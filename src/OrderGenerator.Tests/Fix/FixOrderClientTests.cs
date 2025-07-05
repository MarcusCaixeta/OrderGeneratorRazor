
using Xunit;
using OrderGenerator.Fix;
using OrderGenerator.Models;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderGenerator.Tests.Fix
{
    public class FixOrderClientTests
    {
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
            var client = new FixOrderClient();
            var sessionID = new QuickFix.SessionID("FIX.4.4", "SENDER", "TARGET");
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

            var responseField = typeof(FixOrderClient).GetField("_responseTcs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tcs = new System.Threading.Tasks.TaskCompletionSource<string>();
            responseField.SetValue(client, tcs);

            // Act
            client.OnMessage(report, sessionID);

            // Assert
            Assert.Equal("Order accepted.", tcs.Task.Result);
        }

        [Fact]
        public void OnMessage_ShouldSetResponse_AsRejected_WhenExecTypeIsRejected()
        {
            // Arrange
            var client = new FixOrderClient();
            var sessionID = new QuickFix.SessionID("FIX.4.4", "SENDER", "TARGET");
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

            var responseField = typeof(FixOrderClient).GetField("_responseTcs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tcs = new System.Threading.Tasks.TaskCompletionSource<string>();
            responseField.SetValue(client, tcs);

            // Act
            client.OnMessage(report, sessionID);

            // Assert
            Assert.Equal(" Order rejected.", tcs.Task.Result);
        }
    }
}
