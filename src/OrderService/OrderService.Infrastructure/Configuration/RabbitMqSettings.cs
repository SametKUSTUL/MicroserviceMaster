namespace OrderService.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public string Host { get; set; } = string.Empty;
    public string OrderExchange { get; set; } = string.Empty;
    public string PaymentExchange { get; set; } = string.Empty;
    public string OrderPaymentQueue { get; set; } = string.Empty;
    public string OrderCreatedRoutingKey { get; set; } = string.Empty;
    public string PaymentCompletedRoutingKey { get; set; } = string.Empty;
    public string PaymentFailedRoutingKey { get; set; } = string.Empty;
}
