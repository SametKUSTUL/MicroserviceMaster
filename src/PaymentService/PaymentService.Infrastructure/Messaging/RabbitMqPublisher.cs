using System.Diagnostics;
using System.Text;
using System.Text.Json;
using PaymentService.Application.Services;
using RabbitMQ.Client;

namespace PaymentService.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private static readonly ActivitySource ActivitySource = new("PaymentService");

    public RabbitMqPublisher(string hostName, string exchangeName)
    {
        _exchangeName = exchangeName;
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true);
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity($"RabbitMQ Publish {routingKey}", ActivityKind.Producer);
        
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Headers = new Dictionary<string, object>();
        
        if (Activity.Current != null)
        {
            properties.Headers["traceparent"] = Activity.Current.Id;
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination", _exchangeName);
            activity?.SetTag("messaging.routing_key", routingKey);
        }

        await Task.Run(() => _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body), cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
