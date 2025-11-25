using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductService.Application.Commands;
using ProductService.Application.Services;
using ProductService.Infrastructure.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductService.Infrastructure.Messaging;

public class StockReserveConsumer : IMessageConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockReserveConsumer> _logger;
    private readonly string _queueName;

    public StockReserveConsumer(RabbitMqSettings settings, IServiceProvider serviceProvider, ILogger<StockReserveConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueName = settings.ProductStockQueue;
        
        var factory = new ConnectionFactory { HostName = settings.Host };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(settings.OrderExchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_queueName, settings.OrderExchange, settings.StockReserveRoutingKey);
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
                _logger.LogInformation("Stock reserve message received: {Message}", message);
                var stockEvent = JsonSerializer.Deserialize<StockReserveEvent>(message);
                
                if (stockEvent != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                    foreach (var item in stockEvent.Items)
                    {
                        var command = new UpdateStockCommand(Guid.Parse(item.ProductId), -item.Quantity);
                        await mediator.Send(command, cancellationToken);
                        _logger.LogInformation("Stock decreased for ProductId: {ProductId}, Quantity: {Quantity}", item.ProductId, item.Quantity);
                    }
                    
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock reserve event");
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

public record StockReserveEvent(Guid OrderId, List<StockItem> Items);
public record StockItem(string ProductId, int Quantity);
