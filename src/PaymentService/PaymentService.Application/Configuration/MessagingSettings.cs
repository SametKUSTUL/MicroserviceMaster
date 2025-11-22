namespace PaymentService.Application.Configuration;

public class MessagingSettings
{
    public string PaymentCompletedRoutingKey { get; set; } = "payment.completed";
}
