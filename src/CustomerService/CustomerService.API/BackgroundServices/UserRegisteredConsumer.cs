using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CustomerService.Application.Commands;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CustomerService.API.BackgroundServices;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public UserRegisteredConsumer(IServiceProvider serviceProvider, ILogger<UserRegisteredConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken); // Wait for RabbitMQ to be ready
        
        var factory = new ConnectionFactory { HostName = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "localhost" };
        
        var retryCount = 0;
        while (retryCount < 5)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Retry {RetryCount}/5", retryCount);
                if (retryCount >= 5) throw;
                await Task.Delay(3000, stoppingToken);
            }
        }

        _channel.ExchangeDeclare(exchange: "identity_exchange", type: ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(queue: "customer_user_registered_queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: "customer_user_registered_queue", exchange: "identity_exchange", routingKey: "user.registered");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Extract traceparent and start Activity for distributed tracing
            Activity? activity = null;
            if (ea.BasicProperties?.Headers != null && ea.BasicProperties.Headers.ContainsKey("traceparent"))
            {
                var traceParentBytes = ea.BasicProperties.Headers["traceparent"] as byte[];
                if (traceParentBytes != null)
                {
                    var traceparent = Encoding.UTF8.GetString(traceParentBytes);
                    if (ActivityContext.TryParse(traceparent, null, out var parentContext))
                    {
                        activity = new Activity("ProcessUserRegistered");
                        activity.SetParentId(parentContext.TraceId, parentContext.SpanId, parentContext.TraceFlags);
                        activity.Start();
                    }
                }
            }
            
            activity ??= new Activity("ProcessUserRegistered").Start();

            _logger.LogInformation("[RabbitMQ] Received raw message from queue 'customer_user_registered_queue': {RawMessage}", message);

            try
            {
                var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(message);
                if (userRegisteredEvent != null)
                {
                    _logger.LogInformation("[RabbitMQ] Processing UserRegisteredEvent - Email: {Email}, CustomerId: {CustomerId}", 
                        userRegisteredEvent.Email, userRegisteredEvent.CustomerId);
                    
                    await HandleUserRegisteredAsync(userRegisteredEvent);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    
                    _logger.LogInformation("[RabbitMQ] Successfully processed and acknowledged message for CustomerId: {CustomerId}", 
                        userRegisteredEvent.CustomerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] Error processing user registered event. Raw message: {Message}", message);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queue: "customer_user_registered_queue", autoAck: false, consumer: consumer);

        await Task.CompletedTask;
    }

    private async Task HandleUserRegisteredAsync(UserRegisteredEvent userEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new CreateCustomerCommand(
            CustomerId: userEvent.CustomerId,
            Name: userEvent.Email.Split('@')[0],
            Surname: "User",
            Email: userEvent.Email,
            Phone: "+900000000000"
        );

        await mediator.Send(command);
        _logger.LogInformation("Customer created from user registration: {CustomerId}, {Email}", userEvent.CustomerId, userEvent.Email);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private class UserRegisteredEvent
    {
        public string Email { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}
