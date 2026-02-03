using Serilog;

namespace MultiModuleApp.Logging;

public static class SerilogExtensions
{
    public static void ConfigureSerilog(this LoggerConfiguration loggerConfiguration, string appName)
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", appName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Application} {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Application} {Message:lj}{NewLine}{Exception}",
                shared: true
            );
    }
}
