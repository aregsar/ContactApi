using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;


public static class BuilderExtensions
{
    public const string CustomSourceName = "MyCustomActivitySource";
    public static readonly ActivitySource MyActivitySource = new(CustomSourceName);

    public static TBuilder AddOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry();

        builder.Services.AddOpenTelemetry()
                        .WithTracing(tracing =>
                        {
                            tracing.AddSource(CustomSourceName)
                                    .AddAspNetCoreInstrumentation()
                                    .AddEntityFrameworkCoreInstrumentation()
                                    .AddGrpcClientInstrumentation()
                                    .AddHttpClientInstrumentation();
                        });


        builder.Services.AddOpenTelemetry()
                        .WithMetrics(metrics =>
                        {
                            metrics.AddMeter("Microsoft.EntityFrameworkCore")
                                    .AddAspNetCoreInstrumentation()
                                    .AddHttpClientInstrumentation()
                                    .AddRuntimeInstrumentation();
                        });


        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;

    }
}