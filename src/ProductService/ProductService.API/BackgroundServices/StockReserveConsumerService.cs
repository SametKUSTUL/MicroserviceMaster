using ProductService.Application.Services;

namespace ProductService.API.BackgroundServices;

public class StockReserveConsumerService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<StockReserveConsumerService> _logger;

    public StockReserveConsumerService(IMessageConsumer consumer, ILogger<StockReserveConsumerService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Reserve Consumer Service started");
        await _consumer.StartAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stock Reserve Consumer Service stopping");
        await _consumer.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
