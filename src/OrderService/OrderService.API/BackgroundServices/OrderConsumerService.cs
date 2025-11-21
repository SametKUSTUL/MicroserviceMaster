using OrderService.Application.Services;

namespace OrderService.API.BackgroundServices;

public class OrderConsumerService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<OrderConsumerService> _logger;

    public OrderConsumerService(IMessageConsumer consumer, ILogger<OrderConsumerService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Consumer Service started");
        await _consumer.StartAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order Consumer Service stopping");
        await _consumer.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
