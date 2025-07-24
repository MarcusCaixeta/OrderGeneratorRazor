using Microsoft.Extensions.DependencyInjection;
using OrderGenerator.WorkerService.Interfaces;
using OrderGenerator.WorkerService.Service;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IFixOrderClient, FixOrderClient>();
builder.Services.AddSingleton<IFixSessionManager, FixSessionManager>();
builder.Services.AddSingleton<IFixConfigProvider>(
    new FileFixConfigProvider(Path.Combine("FixConfiguration", "fix.cfg"))
);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
