using PaymentService.Application.Services;

namespace PaymentService.API.BackgroundServices;

public class PaymentConsumerService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<PaymentConsumerService> _logger;

    public PaymentConsumerService(IMessageConsumer consumer, ILogger<PaymentConsumerService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Consumer Service started");
        await _consumer.StartAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment Consumer Service stopping");
        await _consumer.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
