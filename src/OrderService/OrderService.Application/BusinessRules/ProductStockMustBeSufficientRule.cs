using OrderService.Application.Services;

namespace OrderService.Application.BusinessRules;

public class ProductStockMustBeSufficientRule : IBusinessRule
{
    private readonly IProductService _productService;
    private readonly string _productId;
    private readonly int _requestedQuantity;

    public ProductStockMustBeSufficientRule(IProductService productService, string productId, int requestedQuantity)
    {
        _productService = productService;
        _productId = productId;
        _requestedQuantity = requestedQuantity;
    }

    public string ErrorCode => "INSUFFICIENT_STOCK";
    public string ErrorMessage => $"Insufficient stock for product {_productId}. Requested quantity: {_requestedQuantity}";

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductAsync(_productId, cancellationToken);
        return product != null && product.StockQuantity >= _requestedQuantity;
    }
}
