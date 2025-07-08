
using OrderGenerator.Infrastructure.Fix;
using OrderGenerator.Contracts.Models;
using QuickFix.Fields;
using QuickFix.FIX44;
using System.Reflection;
using QuickFix;

namespace OrderGenerator.Tests.Infrastructure.Fix
{
    public class FixOrderClientTests
    {
        private class TestFixOrderClient : FixOrderClient
        {
            public TestFixOrderClient() : base() { }
            public void InjectResponseTcs(TaskCompletionSource<string> tcs)
            {
                var field = typeof(FixOrderClient)
                    .GetField("_responseTcs", BindingFlags.NonPublic | BindingFlags.Instance)!;
                field.SetValue(this, tcs);
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

            // Act
            var order = typeof(FixOrderClient)
                .GetMethod("BuildOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { model }) as NewOrderSingle;

            // Assert
            Assert.NotNull(order);
            Assert.Equal("VALE3", order.GetString(Tags.Symbol));
            Assert.Equal(Side.BUY, order.GetChar(Tags.Side));
            Assert.Equal(25.55m, order.GetDecimal(Tags.Price));
            Assert.Equal(1000m, order.GetDecimal(Tags.OrderQty));
            Assert.Equal('0', order.GetChar(Tags.TimeInForce));

        }

        [Fact]
        public void OnMessage_ShouldSetResponse_AsAccepted_WhenExecTypeIsNew()
        {
            // Arrange
            var client = new TestFixOrderClient();
            var tcs = new TaskCompletionSource<string>();
            client.InjectResponseTcs(tcs);

            var sessionID = new SessionID("FIX.4.4", "GENERATOR", "ACCUMULATOR");

            var report = new ExecutionReport(
                new OrderID("123"),
                new ExecID("456"),
                new ExecType(ExecType.NEW),
                new OrdStatus(OrdStatus.NEW),
                new Symbol("VALE3"),
                new Side(Side.BUY),
                new LeavesQty(100),
                new CumQty(100),
                new AvgPx(25.55m)
            );

            // Act
            client.OnMessage(report, sessionID);

            // Assert
            Assert.True(tcs.Task.IsCompleted);
            Assert.Equal("Order accepted.", tcs.Task.Result);
        }

        [Fact]
        public void OnMessage_ShouldSetResponse_AsRejected_WhenExecTypeIsRejected()
        {
            // Arrange
            var client = new TestFixOrderClient(); // mesma subclasse usada no teste anterior
            var tcs = new TaskCompletionSource<string>();
            client.InjectResponseTcs(tcs);

            var sessionID = new SessionID("FIX.4.4", "GENERATOR", "ACCUMULATOR");

            var report = new ExecutionReport(
                new OrderID("123"),
                new ExecID("456"),
                new ExecType(ExecType.REJECTED),
                new OrdStatus(OrdStatus.REJECTED),
                new Symbol("VALE3"),
                new Side(Side.BUY),
                new LeavesQty(0),
                new CumQty(0),
                new AvgPx(0)
            );

            // Act
            client.OnMessage(report, sessionID);

            // Assert
            Assert.True(tcs.Task.IsCompleted);
            Assert.Equal("Order rejected.", tcs.Task.Result);
        }
        [Fact]
        public void OnMessage_ShouldSetResponse_WithFillExecType()
        {
            // Arrange
            var client = new TestFixOrderClient();
            var tcs = new TaskCompletionSource<string>();
            client.InjectResponseTcs(tcs);

            var sessionID = new SessionID("FIX.4.4", "GENERATOR", "ACCUMULATOR");

            var report = new ExecutionReport(
                new OrderID("ORD123"),
                new ExecID("EXEC456"),
                new ExecType(ExecType.FILL), // ExecType = 2
                new OrdStatus(OrdStatus.FILLED),
                new Symbol("PETR4"),
                new Side(Side.SELL),
                new LeavesQty(0),
                new CumQty(500),
                new AvgPx(30.25m)
            );

            // Act
            client.OnMessage(report, sessionID);

            // Assert
            Assert.True(tcs.Task.IsCompleted);
            Assert.Equal("Return: ExecType = 2", tcs.Task.Result);
        }

    }
}
