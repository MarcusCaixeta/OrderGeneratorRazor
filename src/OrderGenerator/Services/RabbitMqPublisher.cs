using Newtonsoft.Json;
using OrderGenerator.WorkerService.Models;
using RabbitMQ.Client;
using System.Text;

public class RabbitMqPublisher : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",      
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();    
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();      

        _channel.QueueDeclareAsync("order-queue",
            durable: false, exclusive: false, autoDelete: false).GetAwaiter().GetResult(); 
    }

    public async Task PublishAsync(OrderModel order)
    {
        var json = JsonConvert.SerializeObject(order);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: "order-queue",
            body: body
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
