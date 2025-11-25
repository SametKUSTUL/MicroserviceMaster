using OrderService.Application.Services;

namespace OrderService.Application.BusinessRules;

public class ProductMustExistRule : IBusinessRule
{
    private readonly IProductService _productService;
    private readonly string _productId;

    public ProductMustExistRule(IProductService productService, string productId)
    {
        _productService = productService;
        _productId = productId;
    }

    public string ErrorCode => "PRODUCT_NOT_FOUND";
    public string ErrorMessage => $"Product {_productId} does not exist";

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(_productId, out _))
            return false;
            
        return await _productService.ValidateProductAsync(_productId, cancellationToken);
    }
}
