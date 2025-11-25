namespace ProductService.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public string Host { get; set; } = string.Empty;
    public string OrderExchange { get; set; } = string.Empty;
    public string ProductStockQueue { get; set; } = string.Empty;
    public string StockReserveRoutingKey { get; set; } = string.Empty;
}
