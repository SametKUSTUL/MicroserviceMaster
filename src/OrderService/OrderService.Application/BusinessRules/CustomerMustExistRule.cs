using OrderService.Application.Services;

namespace OrderService.Application.BusinessRules;

public class CustomerMustExistRule : IBusinessRule
{
    private readonly ICustomerService _customerService;
    private readonly string _customerId;

    public CustomerMustExistRule(ICustomerService customerService, string customerId)
    {
        _customerService = customerService;
        _customerId = customerId;
    }

    public string ErrorCode => "CUSTOMER_NOT_FOUND";
    public string ErrorMessage => $"Customer with ID '{_customerId}' does not exist";

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return await _customerService.CustomerExistsAsync(_customerId, cancellationToken);
    }
}
