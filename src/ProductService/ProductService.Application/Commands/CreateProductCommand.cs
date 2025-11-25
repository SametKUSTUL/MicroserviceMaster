using MediatR;

namespace ProductService.Application.Commands;

public record CreateProductCommand(string Name, string Description, int StockQuantity, decimal Price) : IRequest<Guid>;
