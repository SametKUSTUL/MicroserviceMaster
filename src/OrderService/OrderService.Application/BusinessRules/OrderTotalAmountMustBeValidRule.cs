using OrderService.Application.Commands;
using OrderService.Application.Constants;
using OrderService.Application.Resources;

namespace OrderService.Application.BusinessRules;

public class OrderTotalAmountMustBeValidRule : IBusinessRule
{
    private readonly IEnumerable<OrderItemDto> _items;
    private const decimal MinAmount = 1.00m;
    private const decimal MaxAmount = 100000.00m;

    public string ErrorCode => ErrorCodes.OrderTotalAmountInvalid;
    public string ErrorMessage => ErrorMessages.GetString(ErrorCodes.OrderTotalAmountInvalid);

    public OrderTotalAmountMustBeValidRule(IEnumerable<OrderItemDto> items)
    {
        _items = items;
    }

    public Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        var totalAmount = _items.Sum(i => i.Price * i.Quantity);
        return Task.FromResult(totalAmount >= MinAmount && totalAmount <= MaxAmount);
    }
}
