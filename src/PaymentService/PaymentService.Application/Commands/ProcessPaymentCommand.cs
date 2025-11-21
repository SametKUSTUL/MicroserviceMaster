using MediatR;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands;

public record ProcessPaymentCommand(Guid OrderId, string CustomerId, decimal Amount) : IRequest<Payment>;
