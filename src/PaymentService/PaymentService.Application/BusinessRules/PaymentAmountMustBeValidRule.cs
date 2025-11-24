using PaymentService.Application.Constants;
using PaymentService.Application.Resources;

namespace PaymentService.Application.BusinessRules;

public class PaymentAmountMustBeValidRule : IBusinessRule
{
    private readonly decimal _amount;
    private const decimal MinAmount = 1.00m;
    private const decimal MaxAmount = 100000.00m;

    public string ErrorCode => ErrorCodes.PaymentAmountInvalid;
    public string ErrorMessage => ErrorMessages.GetString(ErrorCodes.PaymentAmountInvalid);

    public PaymentAmountMustBeValidRule(decimal amount)
    {
        _amount = amount;
    }

    public Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_amount >= MinAmount && _amount <= MaxAmount);
    }
}
