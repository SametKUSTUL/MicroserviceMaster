namespace Identity.Application.Events;

public class UserRegisteredEvent
{
    public string Email { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
