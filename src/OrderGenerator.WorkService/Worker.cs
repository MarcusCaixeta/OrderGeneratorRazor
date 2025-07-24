using Newtonsoft.Json;
using OrderGenerator.WorkerService.Interfaces;
using OrderGenerator.WorkerService.Models;
using RabbitMQ.Client;
using System.Text;

public class Worker : BackgroundService
{
    private IConnection _connection;
    private IChannel _channel;

    private readonly IFixOrderClient _fixClient;

    public Worker( IFixOrderClient fixClient)
    {
        _fixClient = fixClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync("order-queue", durable: false, exclusive: false, autoDelete: false);

        Console.WriteLine("[Worker] Aguardando ordens...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await _channel.BasicGetAsync("order-queue", autoAck: true);

            if (result != null)
            {
                var body = result.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var order = JsonConvert.DeserializeObject<OrderModel>(json);

                Console.WriteLine($"[Worker] Ordem recebida: {order.Symbol}, {order.Side}, {order.Quantity} @ R${order.Price}");

                Console.WriteLine($"[Worker] Processando ordem: {order.Symbol} - {order.Quantity}");


                var result2 = await _fixClient.SendOrder(order);

                Console.WriteLine($"[Worker] Resultado: {result2}");
                
                await Task.Delay(500);
                Console.WriteLine($"[Worker] Ordem processada com sucesso.");
            }
            else
            {
                await Task.Delay(250, stoppingToken);
            }
        }
    }  
}
