namespace OrderService.Application.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductAsync(string productId, CancellationToken cancellationToken = default);
    Task<bool> ValidateProductAsync(string productId, CancellationToken cancellationToken = default);
}

public record ProductDto(Guid Id, string Name, int StockQuantity, decimal CurrentPrice);
