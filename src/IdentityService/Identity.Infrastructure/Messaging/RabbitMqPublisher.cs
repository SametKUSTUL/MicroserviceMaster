using System.Text;
using System.Text.Json;
using Identity.Application.Interfaces;
using RabbitMQ.Client;

namespace Identity.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;

    public RabbitMqPublisher(string hostName, string exchangeName)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _exchangeName = exchangeName;

        _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);
    }

    public void Publish<T>(T message, string routingKey)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.Headers = new Dictionary<string, object>();

        // Add trace context for distributed tracing
        if (System.Diagnostics.Activity.Current != null)
        {
            var activity = System.Diagnostics.Activity.Current;
            var traceFlags = activity.ActivityTraceFlags.HasFlag(System.Diagnostics.ActivityTraceFlags.Recorded) ? "01" : "00";
            var traceParent = $"00-{activity.TraceId}-{activity.SpanId}-{traceFlags}";
            properties.Headers["traceparent"] = Encoding.UTF8.GetBytes(traceParent);
        }

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
