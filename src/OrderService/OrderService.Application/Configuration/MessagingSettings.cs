namespace OrderService.Application.Configuration;

public class MessagingSettings
{
    public string OrderCreatedRoutingKey { get; set; } = "order.created";
    public string StockReserveRoutingKey { get; set; } = "stock.reserve";
}
