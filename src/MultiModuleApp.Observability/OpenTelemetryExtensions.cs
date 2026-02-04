using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter.OpenTelemetryProtocol;
using OpenTelemetry.Exporter.Console;
using OpenTelemetry.Exporter.Otlp;
using System.Reflection;

namespace MultiModuleApp.Observability;

public static class OpenTelemetryExtensions
{
    public static MeterProviderBuilder AddObservability(
        this MeterProviderBuilder builder,
        string serviceName,
        string serviceVersion)
    {
        return builder
            .AddMeter(serviceName)
            .AddRuntimeInstruments()
            .AddProcessInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    }

    public static TracerProviderBuilder AddObservabilityTracing(
        this TracerProviderBuilder builder,
        string serviceName,
        string serviceVersion)
    {
        return builder
            .AddSource(serviceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    }

    public static OpenTelemetrySdkBuilder AddObservabilityResources(
        this OpenTelemetrySdkBuilder builder,
        string serviceName,
        string serviceVersion)
    {
        return builder
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion))
            .ConfigureMetrics(metrics => metrics
                .AddMeter("Application")
                .AddCounter("Requests", "Number of requests")
                .AddHistogram("RequestDuration", "Request duration in milliseconds"))
            .ConfigureTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("Application")
                .AddBatchExportProcessor(new OtlpBatchExportProcessorOptions
                {
                    Endpoint = "http://localhost:4317"
                }));
    }
}
