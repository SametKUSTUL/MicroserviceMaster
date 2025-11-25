using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.Queries;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Product?>;
