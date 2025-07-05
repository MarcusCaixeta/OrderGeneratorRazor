using OrderGenerator.Fix;
using OrderGenerator.Models;
using QuickFix.FIX44;
using QuickFix.Fields;
using Xunit;

namespace OrderGenerator.Tests.Fix
{
    public class FixOrderClientTests
    {
        [Fact]
        public void BuildOrder_ValidModel_ReturnsCorrectOrder()
        {
            // Arrange
            var model = new OrderModel
            {
                Symbol = "VALE3",
                Side = "Compra",
                Quantity = 1000,
                Price = 17.50m
            };

            // Act
            var order = typeof(FixOrderClient)
                        .GetMethod("BuildOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                        ?.Invoke(null, new object[] { model }) as NewOrderSingle;

            // Assert
            Assert.Equal(1000m, order.GetDecimal(Tags.OrderQty));
            Assert.Equal(17.50m, order.GetDecimal(Tags.Price));
            Assert.Equal("VALE3", order.GetString(Tags.Symbol));
            Assert.Equal(Side.BUY, order.GetChar(Tags.Side));
        }
    }
}
