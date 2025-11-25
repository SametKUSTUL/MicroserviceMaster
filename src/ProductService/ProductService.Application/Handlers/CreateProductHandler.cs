using MediatR;
using ProductService.Application.Commands;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            StockQuantity = request.StockQuantity,
            CurrentPrice = request.Price,
            CreatedAt = DateTime.UtcNow,
            PriceHistories = new List<PriceHistory>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Price = request.Price,
                    EffectiveDate = DateTime.UtcNow
                }
            }
        };

        await _repository.CreateAsync(product, cancellationToken);
        return product.Id;
    }
}
