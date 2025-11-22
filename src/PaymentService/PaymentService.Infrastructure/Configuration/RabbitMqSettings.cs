namespace PaymentService.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public string OrderExchange { get; set; } = "order_exchange";
    public string PaymentQueue { get; set; } = "payment_queue";
    public string PaymentExchange { get; set; } = "payment_exchange";
    public string OrderCreatedRoutingKey { get; set; } = "order.created";
    public string PaymentCompletedRoutingKey { get; set; } = "payment.completed";
    public string PaymentFailedRoutingKey { get; set; } = "payment.failed";
}
