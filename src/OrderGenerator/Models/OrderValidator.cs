using FluentValidation;
using OrderGenerator.WorkerService.Models;

namespace OrderGenerator.Validators
{
    public class OrderModelValidator : AbstractValidator<OrderModel>
    {
        private static readonly string[] SymbolsPermitidos = { "PETR4", "VALE3", "VIIA4" };

        public OrderModelValidator()
        {
            RuleFor(x => x.Symbol)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Símbolo é obrigatório.")
                .Must(IsValidSymbol).WithMessage("Símbolo inválido. Use PETR4, VALE3 ou VIIA4.");

            RuleFor(x => x.Side)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Lado é obrigatório.")
                .Must(IsValidSide).WithMessage("Lado deve ser 'Compra' ou 'Venda'.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.")
                .LessThan(100_000).WithMessage("Quantidade deve ser menor que 100.000.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Preço deve ser maior que zero.")
                .LessThan(1_000).WithMessage("Preço deve ser menor que 1.000.")
                .Must(IsPriceValid).WithMessage("Preço deve ser múltiplo de 0.01.");
        }

        private bool IsValidSymbol(string? symbol)
            => !string.IsNullOrWhiteSpace(symbol) && SymbolsPermitidos.Contains(symbol.ToUpperInvariant());

        private bool IsValidSide(string? side)
            => !string.IsNullOrWhiteSpace(side) &&
               (side.Equals("Compra", StringComparison.OrdinalIgnoreCase) ||
                side.Equals("Venda", StringComparison.OrdinalIgnoreCase));

        private bool IsPriceValid(decimal price)
            => price == Math.Round(price, 2);
    }

}