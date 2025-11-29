using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.Services;

namespace OrderService.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerService> _logger;
    private readonly string _customerServiceUrl;

    public CustomerService(HttpClient httpClient, IConfiguration configuration, ILogger<CustomerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _customerServiceUrl = configuration["CustomerService:Url"] ?? "http://customerservice:8080";
    }

    public async Task<bool> CustomerExistsAsync(string customerId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_customerServiceUrl}/api/customers?customerId={customerId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking customer existence for CustomerId: {CustomerId}", customerId);
            return false;
        }
    }
}
