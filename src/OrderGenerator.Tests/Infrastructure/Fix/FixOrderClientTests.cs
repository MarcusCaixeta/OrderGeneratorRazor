
using OrderGenerator.Contracts.Models;
using OrderGenerator.Infrastructure.Fix;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Intrinsics.X86;

namespace OrderGenerator.Tests.Infrastructure.Fix
{
    public class FixOrderClientTests
    {
        private class TestFixOrderClient : FixOrderClient
        {
            public TestFixOrderClient() : base() { }
            public void InjectResponseTcs(string clOrdID, TaskCompletionSource<string> tcs)
            {
                var field = typeof(FixOrderClient)
                    .GetField("_orderResponses", BindingFlags.NonPublic | BindingFlags.Instance)!;

                var dictionary = (ConcurrentDictionary<string, TaskCompletionSource<string>>)field.GetValue(this)!;
                dictionary[clOrdID] = tcs;
            }


            public void SetResponseTcs(TaskCompletionSource<string> tcs)
            {
                typeof(FixOrderClient)
                    .GetField("_responseTcs", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .SetValue(this, tcs);
            }
        }

        [Fact]
        public void BuildOrder_ShouldBuildCorrectFixMessage()
        {
            // Arrange
            var model = new OrderModel
            {
                Symbol = "VALE3",
                Side = "Compra",
                Quantity = 1000,
                Price = 25.55m
            };
            var clOrdID = "ORD123456";

            // Act
            var order = typeof(FixOrderClient)
                .GetMethod("BuildOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { model, clOrdID }) as NewOrderSingle;

            // Assert
            Assert.NotNull(order);
            Assert.Equal("VALE3", order.GetString(Tags.Symbol));
            Assert.Equal(Side.BUY, order.GetChar(Tags.Side));
            Assert.Equal(25.55m, order.GetDecimal(Tags.Price));
            Assert.Equal(1000m, order.GetDecimal(Tags.OrderQty));
            Assert.Equal('0', order.GetChar(Tags.TimeInForce));

        }

        [Fact]
        public void OnMessage_ShouldSetResponse_AsRejected_WhenExecTypeIsRejected()
        {
            // Arrange
            var client = new TestFixOrderClient();

            var tcs = new TaskCompletionSource<string>();
            var clOrdID = "CLREJ001";

            client.InjectResponseTcs(clOrdID, tcs); // injeta no _orderResponses[clOrdID]

            var sessionID = new SessionID("FIX.4.4", "GENERATOR", "ACCUMULATOR");

            var report = new ExecutionReport();
            report.Set(new OrderID("123"));
            report.Set(new ExecID("456"));
            report.Set(new ExecType(ExecType.REJECTED));
            report.Set(new OrdStatus(OrdStatus.REJECTED));
            report.Set(new ClOrdID(clOrdID));
            report.Set(new Symbol("VALE3"));
            report.Set(new Side(Side.BUY));
            report.Set(new LeavesQty(0));
            report.Set(new CumQty(0));
            report.Set(new AvgPx(0));

            // Act
            client.OnMessage(report, sessionID);

            // Assert
            Assert.True(tcs.Task.IsCompleted);
            Assert.Equal("Order rejected.", tcs.Task.Result); // ou "Order rejected." se esse for o texto da sua implementação
        }

        [Fact]
        public void OnMessage_ShouldSetResponse_AsAccepted_WhenExecTypeIsNew()
        {
            // Arrange
            var client = new TestFixOrderClient(); // classe de teste com acesso ao dicionário interno

            var tcs = new TaskCompletionSource<string>();
            var clOrdID = "CL123456";

            // Simula a ordem registrada
            client.InjectResponseTcs(clOrdID, tcs); // faz _orderResponses[clOrdID] = tcs;

            var report = new ExecutionReport();
            report.Set(new OrderID("ORD123"));
            report.Set(new ExecID("EXEC456"));
            report.Set(new ExecType(ExecType.NEW));
            report.Set(new OrdStatus(OrdStatus.NEW));
            report.Set(new ClOrdID(clOrdID)); // deve bater com o clOrdID acima
            report.Set(new Symbol("PETR4"));
            report.Set(new Side(Side.BUY));
            report.Set(new LeavesQty(0));
            report.Set(new CumQty(100));
            report.Set(new AvgPx(10.50m));

            var sessionID = new SessionID("FIX.4.4", "GENERATOR", "ACCUMULATOR");

            // Act
            client.OnMessage(report, sessionID);

            // Assert
            Assert.True(tcs.Task.IsCompleted);
            Assert.Equal("Order accepted.", tcs.Task.Result);
        }


    }
}
