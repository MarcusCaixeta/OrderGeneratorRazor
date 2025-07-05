using Xunit;
using Moq;
using OrderGenerator.Controllers;
using OrderGenerator.Interfaces;
using OrderGenerator.Models;
using Microsoft.AspNetCore.Mvc;

namespace OrderGenerator.Tests.Controllers
{
    public class OrderControllerTests
    {
        [Fact]
        public void Index_Post_ReturnsViewWithResposta()
        {
            // Arrange
            var mockFixClient = new Mock<IFixOrderClient>();
            mockFixClient
                .Setup(c => c.SendOrder(It.IsAny<OrderModel>()))
                .Returns(" Order accepted.");

            var controller = new OrderController(mockFixClient.Object);
            var model = new OrderModel
            {
                Symbol = "PETR4",
                Side = "Compra",
                Quantity = 1000,
                Price = 25.5m
            };

            // Act
            var result = controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(" Order accepted.", controller.ViewBag.Resposta);
            mockFixClient.Verify(c => c.SendOrder(It.IsAny<OrderModel>()), Times.Once);
        }

        [Fact]
        public void Index_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var mockFixClient = new Mock<IFixOrderClient>();
            var controller = new OrderController(mockFixClient.Object);
            controller.ModelState.AddModelError("Price", "Required");

            var model = new OrderModel(); // inválido

            // Act
            var result = controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
            mockFixClient.Verify(c => c.SendOrder(It.IsAny<OrderModel>()), Times.Never);
        }

        [Fact]
        public void Index_ValidModel_ReturnsViewWithResponse()
        {
            // Arrange
            var mockFixClient = new Mock<IFixOrderClient>();
            mockFixClient.Setup(c => c.SendOrder(It.IsAny<OrderModel>())).Returns("Order accepted.");

            var controller = new OrderController(mockFixClient.Object);
            var model = new OrderModel
            {
                Symbol = "PETR4",
                Side = "Compra",
                Quantity = 100,
                Price = 10.0m
            };

            // Act
            var result = controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Order accepted.", controller.ViewBag.Resposta);
        }
    }
}
