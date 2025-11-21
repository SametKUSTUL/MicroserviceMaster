using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands;
using PaymentService.Domain.Repositories;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentRepository _repository;

    public PaymentsController(IMediator mediator, IPaymentRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByIdAsync(id, cancellationToken);
        return payment == null ? NotFound() : Ok(payment);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByOrderIdAsync(orderId, cancellationToken);
        return payment == null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }
}
