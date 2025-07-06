using OrderGenerator.Contracts.Models;

namespace OrderGenerator.Contracts.Interfaces
{
    public interface IFixOrderClient
    {
        Task<string> SendOrder(OrderModel model);
    }
}
