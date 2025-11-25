using Microsoft.Extensions.Logging;
using OrderService.Application.Configuration;
using OrderService.Application.Services;
using OrderService.Domain.Entities;

namespace OrderService.Application.EventPublishers;

public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly MessagingSettings _settings;
    private readonly ILogger<OrderEventPublisher> _logger;

    public OrderEventPublisher(IMessagePublisher messagePublisher, MessagingSettings settings, ILogger<OrderEventPublisher> logger)
    {
        _messagePublisher = messagePublisher;
        _settings = settings;
        _logger = logger;
    }

    public async Task PublishOrderEventsAsync(Order order, CancellationToken cancellationToken)
    {
        await PublishOrderCreatedEventAsync(order, cancellationToken);
        await PublishStockReserveEventAsync(order, cancellationToken);
    }

    private async Task PublishOrderCreatedEventAsync(Order order, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing order.created event for OrderId: {OrderId}", order.Id);
        await _messagePublisher.PublishAsync(_settings.OrderCreatedRoutingKey, new
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount
        }, cancellationToken);
    }

    private async Task PublishStockReserveEventAsync(Order order, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing stock.reserve event for OrderId: {OrderId}", order.Id);
        await _messagePublisher.PublishAsync(_settings.StockReserveRoutingKey, new
        {
            OrderId = order.Id,
            Items = order.Items.Select(i => new { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
        }, cancellationToken);
    }
}
