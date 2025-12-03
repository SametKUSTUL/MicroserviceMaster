using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Security;

namespace Identity.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Identity API", Version = "v1" });
        });

        services.AddDatabase(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddMessaging(configuration);
        services.AddApplicationServices();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IIdentityDbContext>(provider => provider.GetRequiredService<IdentityDbContext>());

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                          ?? throw new InvalidOperationException("JwtSettings configuration is missing");

        services.AddSingleton(jwtSettings);
        services.AddSingleton<JwtTokenGenerator>();

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqExchange = configuration["RabbitMQ:IdentityExchange"] ?? "identity_exchange";

        services.AddScoped<IMessagePublisher>(sp => new RabbitMqPublisher(rabbitMqHost, rabbitMqExchange));

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(Identity.Application.Commands.RegisterUserCommand).Assembly));

        return services;
    }
}
