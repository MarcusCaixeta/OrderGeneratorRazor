using Xunit;
using Moq;
using OrderGenerator.Controllers;
using OrderGenerator.Contracts.Interfaces;
using OrderGenerator.Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace OrderGenerator.Tests.WebApp.Controllers
{
    public class OrderControllerTests
    {
        [Fact]
        public async Task Index_ReturnsView_WithResposta_WhenModelIsValid()
        {
            // Arrange
            var mockFixClient = new Mock<IFixOrderClient>();
            var model = new OrderModel
            {
                Symbol = "PETR4",
                Side = "Compra",
                Quantity = 100,
                Price = 10.5m
            };

            mockFixClient.Setup(f => f.SendOrder(model))
                         .ReturnsAsync("Order accepted.");

            var controller = new OrderController(mockFixClient.Object);

            // Act
            var result = await controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Order accepted.", result?.ViewData["Resposta"]);
        }

        [Fact]
        public async Task Index_ReturnsView_WithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockFixClient = new Mock<IFixOrderClient>();
            var controller = new OrderController(mockFixClient.Object);
            controller.ModelState.AddModelError("Symbol", "Required");

            var model = new OrderModel(); 

            // Act
            var result = await controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result?.Model);
            mockFixClient.Verify(f => f.SendOrder(It.IsAny<OrderModel>()), Times.Never);
        }
    }
}
