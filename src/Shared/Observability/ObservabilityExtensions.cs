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

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithSpan()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                IndexFormat = $"{serviceName.ToLower()}-logs-{{0:yyyy.MM.dd}}",
                NumberOfShards = 1,
                NumberOfReplicas = 0,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                TypeName = null
            }));

        Serilog.Debugging.SelfLog.Enable(Console.Error);
    }

    private static void AddOpenTelemetry(this WebApplicationBuilder builder, string serviceName)
    {
        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(serviceName)
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
            });
    }
}
