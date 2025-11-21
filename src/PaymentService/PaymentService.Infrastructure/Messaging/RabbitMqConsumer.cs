using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Commands;
using PaymentService.Application.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MediatR;

namespace PaymentService.Infrastructure.Messaging;

public class RabbitMqConsumer : IMessageConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(string hostName, IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare("order_exchange", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("payment_queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("payment_queue", "order_exchange", "order.created");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                if (orderEvent != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                    await mediator.Send(new ProcessPaymentCommand(
                        orderEvent.OrderId,
                        orderEvent.CustomerId,
                        orderEvent.TotalAmount
                    ), cancellationToken);
                    
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Payment processed for order {OrderId}", orderEvent.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume("payment_queue", false, consumer);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

public record OrderCreatedEvent(Guid OrderId, string CustomerId, decimal TotalAmount);
