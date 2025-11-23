using PaymentService.Application.Constants;
using PaymentService.Application.Resources;

namespace PaymentService.Application.BusinessRules;

public class CustomerIdMustBeValidRule : IBusinessRule
{
    private readonly string _customerId;

    public string ErrorCode => ErrorCodes.CustomerIdInvalid;
    public string ErrorMessage => ErrorMessages.GetString(ErrorCodes.CustomerIdInvalid);

    public CustomerIdMustBeValidRule(string customerId)
    {
        _customerId = customerId;
    }

    public Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(_customerId));
    }
}
