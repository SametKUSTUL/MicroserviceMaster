using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderService.Application.Services;

namespace OrderService.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;

    public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/products/{productId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }

    public async Task<bool> ValidateProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(productId, cancellationToken);
        return product != null;
    }
}
