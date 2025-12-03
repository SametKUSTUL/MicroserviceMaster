namespace Identity.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public string IdentityExchange { get; set; } = "identity_exchange";
    public string UserRegisteredRoutingKey { get; set; } = "user.registered";
}
