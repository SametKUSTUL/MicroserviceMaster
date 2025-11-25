using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Behaviors;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;

namespace ProductService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddProductServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Commands.CreateProductCommand).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(Application.Commands.CreateProductCommand).Assembly);

        return services;
    }
}
