using MediatR;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Handlers;

public class GetPaymentByOrderIdHandler : IRequestHandler<GetPaymentByOrderIdQuery, Payment?>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByOrderIdHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Payment?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
    }
}
