using MediatR;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Queries;

public record GetPaymentByIdQuery(Guid Id) : IRequest<Payment?>;
