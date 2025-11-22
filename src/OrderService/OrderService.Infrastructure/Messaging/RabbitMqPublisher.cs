using System.Text;
using System.Text.Json;
using OrderService.Application.Services;
using OrderService.Infrastructure.Configuration;
using RabbitMQ.Client;

namespace OrderService.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;

    public RabbitMqPublisher(RabbitMqSettings settings)
    {
        _exchangeName = settings.OrderExchange;
        var factory = new ConnectionFactory { HostName = settings.Host };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true);
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await Task.Run(() => _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body), cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
