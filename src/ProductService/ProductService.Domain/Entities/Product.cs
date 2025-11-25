namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PriceHistory> PriceHistories { get; set; } = new();
}

public class PriceHistory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveDate { get; set; }
    public Product Product { get; set; } = null!;
}
