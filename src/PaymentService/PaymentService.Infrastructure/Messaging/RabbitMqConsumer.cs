using System.Diagnostics;
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
    private readonly string _queueName;
    private static readonly ActivitySource ActivitySource = new("PaymentService");

    public RabbitMqConsumer(string hostName, string exchangeName, string queueName, string routingKey, IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueName = queueName;
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, exchangeName, routingKey);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            string? parentId = null;
            if (ea.BasicProperties?.Headers != null && ea.BasicProperties.Headers.ContainsKey("traceparent"))
            {
                parentId = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["traceparent"]);
            }
            
            Activity? activity = null;
            if (parentId != null)
            {
                activity = ActivitySource.StartActivity("RabbitMQ Process Order Created", ActivityKind.Consumer, parentId);
            }
            else
            {
                activity = ActivitySource.StartActivity("RabbitMQ Process Order Created", ActivityKind.Consumer);
            }
            
            try
            {
                activity?.SetTag("messaging.system", "rabbitmq");
                activity?.SetTag("messaging.destination", _queueName);
                
                _logger.LogInformation("Payment Service Consumer Reading Message: {Message}", message);
                var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                if (orderEvent != null)
                {
                    activity?.SetTag("order.id", orderEvent.OrderId);
                    activity?.SetTag("order.amount", orderEvent.TotalAmount);
                    
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
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Error processing payment message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
            finally
            {
                activity?.Dispose();
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

public record OrderCreatedEvent(Guid OrderId, string CustomerId, decimal TotalAmount);
