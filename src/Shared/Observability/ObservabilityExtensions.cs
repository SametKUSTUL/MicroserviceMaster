using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Elasticsearch;

namespace Observability;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder, string serviceName)
    {
        builder.AddSerilog(serviceName);
        builder.AddOpenTelemetry(serviceName);
        return builder;
    }

    private static void AddSerilog(this WebApplicationBuilder builder, string serviceName)
    {
        var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithSpan()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"logs-{serviceName.ToLower()}-{{0:yyyy.MM.dd}}",
                NumberOfShards = 2,
                NumberOfReplicas = 1
            })
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    private static void AddOpenTelemetry(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(serviceName)
                    .AddConsoleExporter();
            });
    }
}
