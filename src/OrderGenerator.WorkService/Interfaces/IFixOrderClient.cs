using OrderGenerator.WorkerService.Models;

namespace OrderGenerator.WorkerService.Interfaces
{
    public interface IFixOrderClient
    {
        Task<string> SendOrder(OrderModel model);
    }
}
