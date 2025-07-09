using OrderGenerator.Contracts.Interfaces;
using OrderGenerator.Contracts.Models;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using QuickFix;
using System.Collections.Concurrent;

public class FixOrderClient : MessageCracker, IApplication, IFixOrderClient
{
    private readonly IFixSessionManager _sessionManager;
    private TaskCompletionSource<string>? _responseTcs;
    private readonly IInitiator _initiator;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _orderResponses
    = new();


    public FixOrderClient(IFixConfigProvider configProvider, IFixSessionManager sessionManager)
    {
        _sessionManager = sessionManager;

        var settingsPath = configProvider.GetConfigFilePath();
        if (!File.Exists(settingsPath))
            throw new FileNotFoundException($"FIX config file not found at {settingsPath}");

        var settings = new SessionSettings(settingsPath);
        var storeFactory = new FileStoreFactory(settings);
        var logFactory = new FileLogFactory(settings);

        _initiator = new SocketInitiator(this, storeFactory, settings, logFactory);
        _initiator.Start();
    }

    protected FixOrderClient() { }

    public async Task<string> SendOrder(OrderModel model)
    {
        const int maxRetries = 3;
        const int timeout = 5000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (!TryGetActiveSession(out var session))
                    return "Erro: SessionID FIX nulo.";

                EnsureLoggedOn(session);

                if (!await _sessionManager.WaitForLogonAsync(timeout))
                {
                    await RetryLater(attempt, "logon não estabelecido.");
                    continue;
                }

                string clOrdID = GenerateClOrdID();
                var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                _orderResponses[clOrdID] = tcs;

                if (!TrySendOrder(model, clOrdID, session, out var sendError))
                {
                    _orderResponses.TryRemove(clOrdID, out _);
                    return FormatError(attempt, sendError);
                }

                if (await Task.WhenAny(tcs.Task, Task.Delay(timeout)) == tcs.Task)
                {
                    return await tcs.Task;
                }

                _orderResponses.TryRemove(clOrdID, out _);
                return FormatError(attempt, "timeout aguardando resposta.");
            }
            catch (SessionNotFound ex)
            {
                await RetryLater(attempt, $"sessão não encontrada. {ex.Message}");
            }
            catch (Exception ex)
            {
                return FormatError(attempt, $"erro inesperado. {ex.Message}");
            }
        }

        return "Erro: número máximo de tentativas excedido.";
    }


    private bool TryGetActiveSession(out Session session)
    {
        var sessionID = _sessionManager.CurrentSessionID;
        session = sessionID != null ? Session.LookupSession(sessionID) : null;
        return session != null;
    }

    private void EnsureLoggedOn(Session session)
    {
        if (!session.IsLoggedOn)
            session.Logon();
    }

    private async Task RetryLater(int attempt, string reason)
    {
        Console.WriteLine($"Tentativa {attempt}: {reason}");
        await Task.Delay(1000);
    }

    private bool TrySendOrder(OrderModel model, string clOrdID, Session session, out string error)
    {
        var order = BuildOrder(model, clOrdID); // Inclua o ClOrdID no FIX Message
        if (!Session.SendToTarget(order, session.SessionID))
        {
            error = "falha ao enviar ordem FIX.";
            return false;
        }

        error = string.Empty;
        return true;
    }


    private async Task<bool> WaitForResponse(int timeoutMs)
    {
        var completed = await Task.WhenAny(_responseTcs!.Task, Task.Delay(timeoutMs));
        return completed == _responseTcs.Task;
    }

    private static string FormatError(int attempt, string message)
        => $"Tentativa {attempt}: {message}";


    private static NewOrderSingle BuildOrder(OrderModel model, string clOrdID)
    {
        var symbol = string.IsNullOrWhiteSpace(model.Symbol) ? "" : model.Symbol;
        var side = model.Side == "Compra" ? Side.BUY : Side.SELL;

        var order = new NewOrderSingle(
            new ClOrdID(clOrdID), 
            new Symbol(symbol),
            new Side(side),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT)
        );

        order.Set(new OrderQty(model.Quantity));
        order.Set(new Price(model.Price));
        order.Set(new TimeInForce(TimeInForce.DAY));

        return order;
    }


    public void OnMessage(ExecutionReport report, SessionID sessionID)
    {
        if (!report.IsSetField(Tags.ClOrdID))
            return;

        var clOrdID = report.GetString(Tags.ClOrdID);

        string result = report.ExecType.Value switch
        {
            ExecType.NEW => "Order accepted.",
            ExecType.REJECTED => "Order rejected.",
            _ => $"Return: ExecType = {report.ExecType.Value}"
        };

        if (_orderResponses.TryRemove(clOrdID, out var tcs))
        {
            tcs.TrySetResult(result);
        }
    }


    public void OnCreate(SessionID sessionID) => _sessionManager.SetSessionID(sessionID);
    public void OnLogon(SessionID sessionID) => _sessionManager.NotifyLogon(sessionID);
    public void OnLogout(SessionID sessionID) => _sessionManager.NotifyLogout(sessionID);
    public void ToAdmin(QuickFix.Message message, SessionID sessionID) { }
    public void FromAdmin(QuickFix.Message message, SessionID sessionID) { }
    public void ToApp(QuickFix.Message message, SessionID sessionID) => Console.WriteLine("Sent: " + message);
    public void FromApp(QuickFix.Message message, SessionID sessionID) => Crack(message, sessionID);

    private string GenerateClOrdID()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 12); // ou outra lógica sua
    }
}
