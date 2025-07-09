using OrderGenerator.Contracts.Interfaces;
using OrderGenerator.Contracts.Models;
using OrderGenerator.Infrastructure.Fix;
using System.Diagnostics;

namespace OrderGenerator.LoadTests;

public class FixClientLoadTests
{
    private readonly IFixOrderClient _client;

    public FixClientLoadTests()
    {
        var configProvider = new FileFixConfigProvider("FixConfiguration/fix.cfg");
        var sessionManager = new FixSessionManager(); // real implementation
        _client = new FixOrderClient(configProvider, sessionManager); // singleton!
    }

    //[Fact]
    //public async Task Deve_Enviar_100_Ordens_SimulandoChamadasDoFront()
    //{
    //    var tasks = new List<Task<string>>();

    //    for (int i = 0; i < 100; i++)
    //    {
    //        var order = new OrderModel
    //        {
    //            Symbol = "VALE3",
    //            Side = "Compra",
    //            Quantity = 1,
    //            Price = 1m
    //        };

    //        tasks.Add(_client.SendOrder(order));
    //        await Task.Delay(10); 
    //    }

    //    var results = await Task.WhenAll(tasks);

    //    foreach (var result in results)
    //    {
    //        Console.WriteLine(result);
    //        Assert.True(
    //            result.Contains("Order") ,
    //            $"Resultado inesperado: {result}"
    //        );
    //    }
    //}

    //[Fact]
    //public async Task Deve_Enviar_100_Ordens_ComTempoMedio()
    //{
    //    var tasks = new List<Task<(string resultado, long tempoMs)>>();

    //    for (int i = 0; i < 100; i++)
    //    {
    //        tasks.Add(Task.Run(async () =>
    //        {
    //            var stopwatch = Stopwatch.StartNew();

    //            var order = new OrderModel
    //            {
    //                Symbol = "PETR4",
    //                Side = "Compra",
    //                Quantity = 100,
    //                Price = 25.55m
    //            };

    //            string result = await _client.SendOrder(order);
    //            await Task.Delay(10); 
    //            stopwatch.Stop();

    //            return (result, stopwatch.ElapsedMilliseconds);
    //        }));
    //    }

    //    var resultados = await Task.WhenAll(tasks);

    //    // Validação
    //    foreach (var (resultado, tempo) in resultados)
    //    {
    //        Console.WriteLine($"Resultado: {resultado} | Tempo: {tempo} ms");
    //        Assert.True(resultado.Contains("Order"), $"Erro: {resultado}");
    //    }

    //    // Média
    //    double media = resultados.Average(r => r.tempoMs);
    //}
}
