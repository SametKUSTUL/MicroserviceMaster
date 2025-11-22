using MediatR;
using OrderService.Application.Commands;
using OrderService.Application.Configuration;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Order>
{
    private readonly IOrderRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly MessagingSettings _settings;

    public CreateOrderHandler(IOrderRepository repository, IMessagePublisher messagePublisher, MessagingSettings settings)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _settings = settings;
    }

    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

        var result = await _repository.AddAsync(order, cancellationToken);
        
        await _messagePublisher.PublishAsync(_settings.OrderCreatedRoutingKey, new
        {
            OrderId = result.Id,
            CustomerId = result.CustomerId,
            TotalAmount = result.TotalAmount
        }, cancellationToken);

        return result;
    }
}
