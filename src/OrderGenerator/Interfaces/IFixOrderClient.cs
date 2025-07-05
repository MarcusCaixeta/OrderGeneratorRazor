using OrderGenerator.Models;

namespace OrderGenerator.Interfaces
{
    public interface IFixOrderClient
    {
        Task<string> SendOrder(OrderModel model);
    }
}
