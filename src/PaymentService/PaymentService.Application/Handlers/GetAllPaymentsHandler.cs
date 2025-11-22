using MediatR;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Handlers;

public class GetAllPaymentsHandler : IRequestHandler<GetAllPaymentsQuery, IEnumerable<Payment>>
{
    private readonly IPaymentRepository _repository;

    public GetAllPaymentsHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Payment>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
