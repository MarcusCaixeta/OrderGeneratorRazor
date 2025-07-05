using OrderGenerator.Interfaces;
using OrderGenerator.Models;
using QuickFix;
using QuickFix.Fields;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

public class FixOrderClient : MessageCracker, IApplication, IFixOrderClient
{
    private SessionID _sessionID;
    private TaskCompletionSource<string> _responseTcs;

    public string SendOrder(OrderModel model)
    {
        using var initiator = CreateFixSession();

        initiator.Start();
        Thread.Sleep(1000);

        var order = BuildOrder(model);

        if (_sessionID == null)
            return " FIX sessionID null.";

        Session.SendToTarget(order, _sessionID);

        return WaitForExecutionResponse();
    }

    private SocketInitiator CreateFixSession()
    {
        var settings = new SessionSettings("Fix/fix.cfg");
        var storeFactory = new FileStoreFactory(settings);
        var logFactory = new FileLogFactory(settings);

        return new SocketInitiator(this, storeFactory, settings, logFactory);
    }

    private QuickFix.FIX44.NewOrderSingle BuildOrder(OrderModel model)
    {
        var side = model.Side == "Compra" ? Side.BUY : Side.SELL;

        var order = new QuickFix.FIX44.NewOrderSingle(new ClOrdID(Guid.NewGuid().ToString()), new Symbol(model.Symbol), new Side(side),
            new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT));

        order.Set(new OrderQty(model.Quantity));
        order.Set(new Price(model.Price));
        order.Set(new TimeInForce(TimeInForce.DAY));

        return order;
    }

    private string WaitForExecutionResponse()
    {
        _responseTcs = new TaskCompletionSource<string>();

        if (_responseTcs.Task.Wait(5000))
            return _responseTcs.Task.Result;

        return "Timeout ExecutionReport.";
    }

    // Callback from server (ExecutionReport)
    public void OnMessage(QuickFix.FIX44.ExecutionReport report, SessionID sessionID)
    {
        var result = report.ExecType.Value switch
        {
            ExecType.NEW => "Order accepted.",
            ExecType.REJECTED => " Order rejected.",
            _ => $" Return: ExecType = {report.ExecType.Value}"
        };

        _responseTcs?.TrySetResult(result);
    }

    // IApplication lifecycle
    public void OnCreate(SessionID sessionID) => _sessionID = sessionID;
    public void OnLogon(SessionID sessionID) => _sessionID = sessionID;
    public void OnLogout(SessionID sessionID) { }
    public void ToAdmin(Message message, SessionID sessionID) { }
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void ToApp(Message message, SessionID sessionID) => Console.WriteLine(" Sent: " + message);
    public void FromApp(Message message, SessionID sessionID) => Crack(message, sessionID);
}
