using OrderService.Application.Commands;
using OrderService.Domain.Entities;

namespace OrderService.Application.Factories;

public class OrderFactory : IOrderFactory
{
    public Order CreateOrder(CreateOrderCommand command)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = command.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);
        return order;
    }
}
