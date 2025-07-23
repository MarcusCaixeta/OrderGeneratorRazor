using OrderGenerator.WorkerService.Models;

public interface IOrderQueue
{
    void Enqueue(OrderModel order);
    bool TryDequeue(out OrderModel order);
    bool HasItems();
}