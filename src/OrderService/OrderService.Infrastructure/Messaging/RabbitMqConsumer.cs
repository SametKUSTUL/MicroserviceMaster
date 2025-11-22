using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Infrastructure.Messaging;

public class RabbitMqConsumer : IMessageConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly string _queueName;

    public RabbitMqConsumer(RabbitMqSettings settings, IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueName = settings.OrderPaymentQueue;
        var factory = new ConnectionFactory { HostName = settings.Host };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(settings.PaymentExchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_queueName, settings.PaymentExchange, settings.PaymentCompletedRoutingKey);
        _channel.QueueBind(_queueName, settings.PaymentExchange, settings.PaymentFailedRoutingKey);
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
                var paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(message);
                if (paymentEvent != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    
                    var order = await repository.GetByIdAsync(paymentEvent.OrderId, cancellationToken);
                    if (order != null)
                    {
                        order.Status = paymentEvent.Status == "Completed" ? OrderStatus.Paid : OrderStatus.Pending;
                        order.UpdatedAt = DateTime.UtcNow;
                        await repository.UpdateAsync(order, cancellationToken);
                        
                        _logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, order.Status);
                    }
                    
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment event");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(_queueName, false, consumer);
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

public record PaymentEvent(Guid PaymentId, Guid OrderId, decimal Amount, string Status);
