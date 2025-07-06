using OrderGenerator.Contracts.Interfaces;
using OrderGenerator.Contracts.Models;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderGenerator.Infrastructure.Fix
{
    public class FixOrderClient : MessageCracker, IApplication, IFixOrderClient
    {
        private SessionID? _sessionID;
        private TaskCompletionSource<string>? _responseTcs;
        private TaskCompletionSource<bool>? _logonTcs;

        public async Task<string> SendOrder(OrderModel model)
        {
            try
            {
                InitializeLogonAwaiter();

                using var initiator = CreateFixSession();
                initiator.Start();

                if (!await WaitForLogonAsync())
                    return "Timeout during FIX logon.";

                if (_sessionID is null)
                    return "FIX session ID is null.";

                var order = BuildOrder(model);
                SendToFix(order);

                return await WaitForExecutionResponseAsync();
            }
            catch (Exception ex)
            {
                return $"Erro ao enviar Order: {ex.Message}";
            }
        }

        private void InitializeLogonAwaiter()
        {
            _logonTcs = new TaskCompletionSource<bool>();
        }

        private async Task<bool> WaitForLogonAsync()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var resultTask = await Task.WhenAny(_logonTcs!.Task, Task.Delay(timeout));
            return resultTask == _logonTcs.Task && _logonTcs.Task.Result;
        }

        private void SendToFix(NewOrderSingle order)
        {
            if (_sessionID != null)
                Session.SendToTarget(order, _sessionID);
        }

        private SocketInitiator CreateFixSession()
        {
            var settings = new SessionSettings("Fix/fix.cfg");
            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);

            return new SocketInitiator(this, storeFactory, settings, logFactory);
        }

        private static QuickFix.FIX44.NewOrderSingle BuildOrder(OrderModel model)
        {
            var symbol = string.IsNullOrWhiteSpace(model.Symbol) ? "PETR4" : model.Symbol;
            var side = model.Side == "Compra" ? Side.BUY : Side.SELL;
            var clOrdId = new ClOrdID(Guid.NewGuid().ToString());
            var transactTime = new TransactTime(DateTime.UtcNow);
            var ordType = new OrdType(OrdType.LIMIT);

            var order = new NewOrderSingle(clOrdId, new Symbol(symbol), new Side(side), transactTime, ordType)
            {
                OrderQty = new OrderQty(model.Quantity),
                Price = new Price(model.Price),
                TimeInForce = new TimeInForce(TimeInForce.DAY)
            };
            order.Set(new OrderQty(model.Quantity));
            order.Set(new Price(model.Price));
            order.Set(new TimeInForce(TimeInForce.DAY));

            return order;
        }

        private async Task<string> WaitForExecutionResponseAsync()
        {
            _responseTcs = new TaskCompletionSource<string>();

            var completedTask = await Task.WhenAny(_responseTcs.Task, Task.Delay(5000));

            if (completedTask == _responseTcs.Task)
                return await _responseTcs.Task;

            return "Timeout ExecutionReport.";
        }


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

        public void OnLogon(SessionID sessionID)
        {
            _sessionID = sessionID;
            _logonTcs?.TrySetResult(true);
        }
        public void OnLogout(SessionID sessionID) { }
        public void ToAdmin(QuickFix.Message message, SessionID sessionID) { }
        public void FromAdmin(QuickFix.Message message, SessionID sessionID) { }
        public void ToApp(QuickFix.Message message, SessionID sessionID) => Console.WriteLine(" Sent: " + message);
        public void FromApp(QuickFix.Message message, SessionID sessionID) => Crack(message, sessionID);
    }
}
