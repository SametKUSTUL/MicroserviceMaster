using MediatR;

namespace ProductService.Application.Commands;

public record UpdateStockCommand(Guid ProductId, int Quantity) : IRequest<Unit>;
