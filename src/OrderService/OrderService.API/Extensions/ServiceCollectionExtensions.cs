using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.API.BackgroundServices;
using OrderService.Application.Services;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Configuration;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Services;

namespace OrderService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDatabase(configuration);
        services.AddRepositories();
        services.AddMessaging(configuration);
        services.AddMediator(configuration);
        services.AddBackgroundServices();
        services.AddExternalServices(configuration);

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
        services.AddSingleton(rabbitMqSettings);

        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();

        return services;
    }

    private static IServiceCollection AddMediator(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingSettings = configuration.GetSection("Messaging").Get<Application.Configuration.MessagingSettings>() ?? new Application.Configuration.MessagingSettings();
        services.AddSingleton(messagingSettings);
        
        services.AddScoped<Application.Factories.IOrderFactory, Application.Factories.OrderFactory>();
        services.AddScoped<Application.Services.IOrderEventPublisher, Application.EventPublishers.OrderEventPublisher>();
        
        services.AddValidatorsFromAssembly(typeof(Application.Commands.CreateOrderCommand).Assembly);
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Commands.CreateOrderCommand).Assembly);
            cfg.AddOpenBehavior(typeof(Application.Behaviors.ValidationBehavior<,>));
        });
        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<OrderConsumerService>();
        return services;
    }

    private static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IProductService, ProductService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ProductService:Url"] ?? "http://localhost:5003");
        });
        return services;
    }
}
