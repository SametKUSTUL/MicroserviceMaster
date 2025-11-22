using MediatR;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Queries;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<Payment?>;
