namespace ProductService.Application.Services;

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
}
