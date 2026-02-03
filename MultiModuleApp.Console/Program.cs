using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiModuleApp.Audit.Interfaces;
using MultiModuleApp.Audit.Services;
using MultiModuleApp.Core.Entities;
using MultiModuleApp.Core.Interfaces;
using MultiModuleApp.Infrastructure.Data;
using MultiModuleApp.Infrastructure.Services;
using MultiModuleApp.Logging;
using MultiModuleApp.Observability;
using MultiModuleApp.Sync.Interfaces;
using MultiModuleApp.Sync.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

// Configure Serilog
var builder = Host.CreateApplicationBuilder(args);

// Determine environment
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Starting MultiModuleApp in {environment} environment...");

// Load configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Validate configuration
await ValidateConfigurationAsync(builder.Configuration);

// Configure Serilog
var loggerConfig = new LoggerConfiguration();
loggerConfig.ConfigureSerilog("MultiModuleApp");
Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

// Configure Services
ConfigureServices(builder.Services, builder.Configuration);

// Configure Observability
ConfigureObservability(builder, builder.Configuration);

// Build and run
var host = builder.Build();

try
{
    Log.Information("Application starting up...");
    Log.Information("Environment: {Environment}", environment);
    Log.Information("Service Name: {ServiceName}", builder.Configuration["OpenTelemetry:ServiceName"]);

    // Display configuration summary
    await DisplayConfigurationSummaryAsync(host.Services, builder.Configuration);

    // Run the application
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Database
    services.AddDbContext<MySqlDbContext>(options =>
    {
        var connectionString = configuration.GetConnectionString("MySqlConnection");
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    });

    services.AddSingleton<IOracleRepository>(sp =>
    {
        var connectionString = configuration.GetConnectionString("OracleConnection");
        return new OracleRepository(connectionString);
    });

    // Core Services
    services.AddScoped<IProductService, ProductService>();

    // Audit
    var auditConfig = configuration.GetSection("Audit");
    services.AddSingleton<IAuditService>(sp =>
    {
        var logFilePath = auditConfig["LogFilePath"] ?? "logs/audit.log";
        return new AuditService(logFilePath);
    });

    // Sync
    services.AddScoped<IDataSyncService, DataSyncService>();

    // Background Services
    services.AddHostedService<ApplicationWorker>();
}

void ConfigureObservability(HostApplicationBuilder builder, IConfiguration configuration)
{
    var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "MultiModuleApp";
    var serviceVersion = configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

    // Metrics
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName, serviceVersion: serviceVersion))
        .AddMeter(serviceName)
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
        .AddConsoleExporter();

    // Tracing
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName, serviceVersion: serviceVersion))
        .AddSource(serviceName)
        .AddSource("Microsoft.AspNetCore")
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter();
}

async Task ValidateConfigurationAsync(IConfiguration configuration)
{
    Console.WriteLine("\n=== Configuration Validation ===");

    var errors = new List<string>();

    // Check connection strings
    var oracleConn = configuration.GetConnectionString("OracleConnection");
    var mysqlConn = configuration.GetConnectionString("MySqlConnection");

    if (string.IsNullOrEmpty(oracleConn))
        errors.Add("Oracle connection string is missing");
    else
        Console.WriteLine("✓ Oracle connection string configured");

    if (string.IsNullOrEmpty(mysqlConn))
        errors.Add("MySQL connection string is missing");
    else
        Console.WriteLine("✓ MySQL connection string configured");

    // Check audit configuration
    var auditEnabled = configuration.GetValue<bool>("Audit:Enabled");
    Console.WriteLine($"✓ Audit service {(auditEnabled ? "enabled" : "disabled")}");

    // Check OpenTelemetry
    var otelServiceName = configuration["OpenTelemetry:ServiceName"];
    if (string.IsNullOrEmpty(otelServiceName))
        errors.Add("OpenTelemetry service name is missing");
    else
        Console.WriteLine($"✓ OpenTelemetry service name: {otelServiceName}");

    if (errors.Any())
    {
        Console.WriteLine("\n⚠ Configuration errors:");
        foreach (var error in errors)
        {
            Console.WriteLine($"  - {error}");
        }
    }
    else
    {
        Console.WriteLine("\n✓ All configuration checks passed");
    }

    Console.WriteLine("=================================\n");
}

async Task DisplayConfigurationSummaryAsync(IServiceProvider serviceProvider, IConfiguration configuration)
{
    Console.WriteLine("\n=== Configuration Summary ===");
    Console.WriteLine($"Environment: {configuration["Environment"]}");
    Console.WriteLine($"Service Name: {configuration["OpenTelemetry:ServiceName"]}");
    Console.WriteLine($"Service Version: {configuration["OpenTelemetry:ServiceVersion"]}");
    Console.WriteLine($"Audit Enabled: {configuration.GetValue<bool>("Audit:Enabled")}");
    Console.WriteLine($"Log Level: {configuration["Serilog:MinimumLevel:Default"]}");
    Console.WriteLine("=============================\n");
}

// Application Worker
public class ApplicationWorker : BackgroundService
{
    private readonly ILogger<ApplicationWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ApplicationWorker(
        ILogger<ApplicationWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ApplicationWorker started at: {time}", DateTimeOffset.Now);

        try
        {
            // Perform startup tasks
            using var scope = _serviceProvider.CreateScope();
            await PerformStartupTasksAsync(scope.ServiceProvider, stoppingToken);

            // Keep the application running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                
                // Perform periodic health checks
                await PerformHealthCheckAsync(scope.ServiceProvider);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApplicationWorker failed");
            throw;
        }
        finally
        {
            _logger.LogInformation("ApplicationWorker stopped at: {time}", DateTimeOffset.Now);
        }
    }

    private async Task PerformStartupTasksAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var auditService = services.GetRequiredService<IAuditService>();
        
        await auditService.LogActionAsync(
            "ApplicationStartup",
            "Application",
            userId: "System",
            details: $"Application started in {_configuration["Environment"]} environment"
        );

        // Log product count from MySQL
        try
        {
            var productService = services.GetRequiredService<IProductService>();
            var products = await productService.GetAllAsync();
            _logger.LogInformation("Found {Count} products in database", products.Count());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve products from database");
        }
    }

    private async Task PerformHealthCheckAsync(IServiceProvider services)
    {
        // Simple health check - log that the application is still running
        _logger.LogDebug("Health check passed at: {time}", DateTimeOffset.Now);
    }
}
