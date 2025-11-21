using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Tests.Repositories;

[TestFixture]
public class PaymentRepositoryTests
{
    private PaymentDbContext _context;
    private PaymentRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PaymentDbContext(options);
        _repository = new PaymentRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_ShouldAddPayment_ToDatabase()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = "customer-123",
            Amount = 100.00m,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(payment, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(payment.Id));

        var savedPayment = await _context.Payments.FindAsync(payment.Id);
        Assert.That(savedPayment, Is.Not.Null);
    }

    [Test]
    public async Task GetByOrderIdAsync_ShouldReturnPayment_WhenExists()
    {
        var orderId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = "customer-456",
            Amount = 200.00m,
            Status = PaymentStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByOrderIdAsync(orderId, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.OrderId, Is.EqualTo(orderId));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllPayments()
    {
        var payments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), CustomerId = "customer-1", Amount = 100, Status = PaymentStatus.Completed, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), CustomerId = "customer-2", Amount = 200, Status = PaymentStatus.Pending, CreatedAt = DateTime.UtcNow }
        };

        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(CancellationToken.None);

        Assert.That(result.Count(), Is.EqualTo(2));
    }
}
