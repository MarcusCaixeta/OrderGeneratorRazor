using QuickFix;
using QuickFix.Fields;
using QuickFix.Transport;
using OrderGenerator.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using QuickFix.Logger;
using QuickFix.Store;

namespace OrderGenerator.Fix
{
    public class FixClient : MessageCracker, IApplication
    {
        private SessionID _sessionID;
        private TaskCompletionSource<string> _responseTcs;

        public string SendOrder(OrderModel model)
        {
            SocketInitiator? initiator = null;

            try
            {
                _responseTcs = new TaskCompletionSource<string>();

                var settings = new SessionSettings("Fix/fix.cfg");
                var storeFactory = new FileStoreFactory(settings);
                var logFactory = new FileLogFactory(settings);

                initiator = new SocketInitiator(this, storeFactory, settings, logFactory);
                initiator.Start();

                Thread.Sleep(1000); // Espera o logon

                var order = new QuickFix.FIX44.NewOrderSingle(
                    new ClOrdID(Guid.NewGuid().ToString()),
                    new Symbol(model.Symbol),
                    new Side(model.Side == "Compra" ? Side.BUY : Side.SELL),
                    new TransactTime(DateTime.UtcNow),
                    new OrdType(OrdType.LIMIT));

                order.Set(new OrderQty(model.Quantity));
                order.Set(new Price(model.Price));
                order.Set(new TimeInForce(TimeInForce.DAY));

                if (_sessionID != null)
                    Session.SendToTarget(order, _sessionID);
                else
                    return "Erro: sessão FIX não estabelecida.";

                if (_responseTcs.Task.Wait(5000))
                    return _responseTcs.Task.Result;
                else
                    return "Erro: timeout ao aguardar resposta do servidor.";
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }
            finally
            {
                if (initiator != null)
                {
                    initiator.Stop(); // 🔓 Libera arquivos de sessão FIX
                }
            }
        }


        // Executado quando o servidor envia uma resposta (ExecutionReport)
        public void OnMessage(QuickFix.FIX44.ExecutionReport report, SessionID sessionID)
        {
            var execType = report.ExecType.Value;

            if (execType == ExecType.NEW)
                _responseTcs?.TrySetResult("✅ Ordem aceita.");
            else if (execType == ExecType.REJECTED)
                _responseTcs?.TrySetResult("❌ Ordem rejeitada.");
            else
                _responseTcs?.TrySetResult($"ℹ️ Resposta recebida: ExecType = {execType}");
        }

        // FIX plumbing
        public void OnCreate(SessionID sessionID) => _sessionID = sessionID;
        public void OnLogon(SessionID sessionID) => _sessionID = sessionID;
        public void OnLogout(SessionID sessionID) { }
        public void ToAdmin(Message message, SessionID sessionID) { }
        public void ToApp(Message message, SessionID sessionID) => Console.WriteLine("Enviado: " + message);
        public void FromAdmin(Message message, SessionID sessionID) { }
        public void FromApp(Message message, SessionID sessionID) => Crack(message, sessionID);
    }
}
