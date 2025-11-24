using OrderService.Application.Commands;
using OrderService.Application.Constants;
using OrderService.Application.Resources;

namespace OrderService.Application.BusinessRules;

public class OrderMustHaveItemsRule : IBusinessRule
{
    private readonly IEnumerable<OrderItemDto> _items;

    public string ErrorCode => ErrorCodes.OrderMustHaveItems;
    public string ErrorMessage => ErrorMessages.GetString(ErrorCodes.OrderMustHaveItems);

    public OrderMustHaveItemsRule(IEnumerable<OrderItemDto> items)
    {
        _items = items;
    }

    public Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items?.Any() == true);
    }
}
