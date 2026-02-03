using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
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
            .AddRuntimeInstrumentation()
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
}
