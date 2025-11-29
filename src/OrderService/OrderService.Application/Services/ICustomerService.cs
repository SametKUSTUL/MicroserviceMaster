namespace OrderService.Application.Services;

public interface ICustomerService
{
    Task<bool> CustomerExistsAsync(string customerId, CancellationToken cancellationToken);
}
