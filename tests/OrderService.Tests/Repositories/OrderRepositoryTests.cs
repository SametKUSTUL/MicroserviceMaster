using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Tests.Repositories;

[TestFixture]
public class OrderRepositoryTests
{
    private OrderDbContext _context;
    private OrderRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);
        _repository = new OrderRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_ShouldAddOrder_ToDatabase()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "customer-123",
            TotalAmount = 100.00m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = "product-1", Quantity = 2, Price = 50.00m }
            }
        };

        var result = await _repository.AddAsync(order, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(order.Id));

        var savedOrder = await _context.Orders.FindAsync(order.Id);
        Assert.That(savedOrder, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "customer-456",
            TotalAmount = 200.00m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(order.Id, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(order.Id));
        Assert.That(result.CustomerId, Is.EqualTo("customer-456"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllOrders()
    {
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), CustomerId = "customer-1", TotalAmount = 100, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), CustomerId = "customer-2", TotalAmount = 200, Status = OrderStatus.Paid, CreatedAt = DateTime.UtcNow }
        };

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(CancellationToken.None);

        Assert.That(result.Count(), Is.EqualTo(2));
    }
}
