using OrderGenerator.Validators;
using FluentValidation;
using OrderGenerator.WorkerService.Interfaces;
using OrderGenerator.WorkerService.Service;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IFixConfigProvider>(
    new FileFixConfigProvider(Path.Combine("FixConfiguration", "fix.cfg"))
);
builder.Services.AddSingleton<IFixSessionManager, FixSessionManager>();

builder.Services.AddSingleton<IFixOrderClient, FixOrderClient>();

builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddControllersWithViews();
builder.Services.AddValidatorsFromAssemblyContaining<OrderModelValidator>(); 



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Order}/{action=Index}/{id?}");

app.Run();
