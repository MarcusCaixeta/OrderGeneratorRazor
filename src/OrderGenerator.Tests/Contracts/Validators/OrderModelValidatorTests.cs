using FluentValidation.TestHelper;
using OrderGenerator.Contracts.Models;
using OrderGenerator.Validators;
using Xunit;

namespace OrderGenerator.Tests.Contracts.Validators;

public class OrderModelValidatorTests
{
    private readonly OrderModelValidator _validator = new();

    [Fact]
    public void ValidOrder_ShouldPass()
    {
        var model = new OrderModel
        {
            Symbol = "PETR4",
            Side = "Compra",
            Quantity = 100,
            Price = 50.00m
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Compra", 100, 50.00)]
    [InlineData(null, "Compra", 100, 50.00)]
    public void InvalidSymbol_ShouldFail(string symbol, string side, int quantity, decimal price)
    {
        var model = new OrderModel { Symbol = symbol, Side = side, Quantity = quantity, Price = price };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Symbol);
    }

    [Theory]
    [InlineData("PETR4", "", 100, 50.00)]
    [InlineData("PETR4", null, 100, 50.00)]
    [InlineData("PETR4", "Outro", 100, 50.00)]
    public void InvalidSide_ShouldFail(string symbol, string side, int quantity, decimal price)
    {
        var model = new OrderModel { Symbol = symbol, Side = side, Quantity = quantity, Price = price };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Side);
    }

    [Theory]
    [InlineData("PETR4", "Compra", 0, 50.00)]
    [InlineData("PETR4", "Compra", 100000, 50.00)]
    public void InvalidQuantity_ShouldFail(string symbol, string side, int quantity, decimal price)
    {
        var model = new OrderModel { Symbol = symbol, Side = side, Quantity = quantity, Price = price };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData("PETR4", "Compra", 10, -1)]
    [InlineData("PETR4", "Compra", 10, 0)]
    [InlineData("PETR4", "Compra", 10, 1000.01)]
    [InlineData("PETR4", "Compra", 10, 0.001)]
    public void InvalidPrice_ShouldFail(string symbol, string side, int quantity, decimal price)
    {
        var model = new OrderModel { Symbol = symbol, Side = side, Quantity = quantity, Price = price };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData(100.001)]
    [InlineData(99.999)]
    [InlineData(49.1234)]
    [InlineData(20.555)]
    public void InvalidPrice_ShouldFailPrice(decimal price)
    {
        var model = new OrderModel
        {
            Symbol = "PETR4",
            Side = "Compra",
            Quantity = 10,
            Price = price
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("Preço deve ser múltiplo de 0.01.");
    }

    [Theory]
    [InlineData(100.00)]
    [InlineData(0.01)]
    [InlineData(999.99)]
    [InlineData(50.55)]
    public void ValidPrice_ShouldPass(decimal price)
    {
        var model = new OrderModel
        {
            Symbol = "PETR4",
            Side = "Compra",
            Quantity = 10,
            Price = price
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }
}
