using Microsoft.EntityFrameworkCore;
using PaymentService.API.BackgroundServices;
using PaymentService.Application.Services;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<IMessagePublisher>(sp => 
    new RabbitMqPublisher(builder.Configuration["RabbitMQ:Host"] ?? "localhost"));
builder.Services.AddSingleton<IMessageConsumer>(sp => 
    new RabbitMqConsumer(builder.Configuration["RabbitMQ:Host"] ?? "localhost", sp, sp.GetRequiredService<ILogger<RabbitMqConsumer>>()));

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(PaymentService.Application.Commands.ProcessPaymentCommand).Assembly));

builder.Services.AddHostedService<PaymentConsumerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
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
