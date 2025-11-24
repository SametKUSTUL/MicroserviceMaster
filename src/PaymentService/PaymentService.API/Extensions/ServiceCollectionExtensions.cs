using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Services;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Configuration;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddRepositories();
        services.AddMessaging(configuration);
        services.AddMediator(configuration);

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
        services.AddSingleton(rabbitMqSettings);

        services.AddSingleton<IMessagePublisher>(sp =>
        {
            var settings = sp.GetRequiredService<RabbitMqSettings>();
            return new RabbitMqPublisher(settings.Host, settings.PaymentExchange);
        });

        services.AddSingleton<IMessageConsumer>(sp =>
        {
            var settings = sp.GetRequiredService<RabbitMqSettings>();
            var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer>>();
            return new RabbitMqConsumer(settings.Host, settings.OrderExchange, settings.PaymentQueue, settings.OrderCreatedRoutingKey, sp, logger);
        });

        return services;
    }

    private static IServiceCollection AddMediator(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingSettings = configuration.GetSection("Messaging").Get<Application.Configuration.MessagingSettings>() ?? new Application.Configuration.MessagingSettings();
        services.AddSingleton(messagingSettings);
        
        services.AddValidatorsFromAssembly(typeof(Application.Commands.ProcessPaymentCommand).Assembly);
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Commands.ProcessPaymentCommand).Assembly);
            cfg.AddOpenBehavior(typeof(Application.Behaviors.ValidationBehavior<,>));
        });
        return services;
    }
}
