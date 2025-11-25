using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public interface IOrderEventPublisher
{
    Task PublishOrderEventsAsync(Order order, CancellationToken cancellationToken);
}
