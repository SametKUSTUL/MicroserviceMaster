using Microsoft.EntityFrameworkCore;
using OrderService.API.BackgroundServices;
using OrderService.Application.Services;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IMessagePublisher>(sp => 
    new RabbitMqPublisher(builder.Configuration["RabbitMQ:Host"] ?? "localhost"));
builder.Services.AddSingleton<IMessageConsumer>(sp => 
    new RabbitMqConsumer(builder.Configuration["RabbitMQ:Host"] ?? "localhost", sp, sp.GetRequiredService<ILogger<RabbitMqConsumer>>()));

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(OrderService.Application.Commands.CreateOrderCommand).Assembly));

builder.Services.AddHostedService<OrderConsumerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
