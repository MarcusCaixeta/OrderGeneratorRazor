using OrderGenerator.Models;

namespace OrderGenerator.Interfaces
{
    public interface IFixOrderClient
    {
        string SendOrder(OrderModel model);
    }
}
