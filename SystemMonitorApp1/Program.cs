using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemMonitorApp1.Interfaces;
using SystemMonitorApp1.Platform;
using SystemMonitorApp1.Plugins;
using SystemMonitorApp1.Services;

Console.WriteLine("[Program] Starting SystemMonitorApp1...");

// ─── Configuration ─────────────────────────────────────────────────────────
Console.WriteLine("[Program] Loading configuration from appsettings.json...");
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Console.WriteLine($"[Program] MonitoringInterval: {configuration["MonitoringInterval"]}s");
Console.WriteLine($"[Program] ApiUrl: {configuration["ApiUrl"]}");

// ─── Dependency Injection ──────────────────────────────────────────────────
Console.WriteLine("[Program] Registering services...");
var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddHttpClient("MonitorApi", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "SystemMonitorApp1/1.0");
    client.Timeout = TimeSpan.FromSeconds(15);
});

services.AddSingleton<IPlatformMetricsProvider>(_ => PlatformMetricsProviderFactory.Create());

// Register plugins — add new plugins here without changing core logic
services.AddSingleton<IMonitorPlugin, FileLoggerPlugin>();
services.AddSingleton<IMonitorPlugin, ApiPlugin>();
services.AddSingleton<ISystemMonitorService, SystemMonitorService>();

var serviceProvider = services.BuildServiceProvider();
Console.WriteLine("[Program] All services registered successfully.");

// ─── Cancellation ──────────────────────────────────────────────────────────
using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\n[Program] Ctrl+C detected. Shutting down...");
    cts.Cancel();
};

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[Program] Unhandled exception: {e.ExceptionObject}");
    Console.ResetColor();
};

// ─── Start Monitoring ──────────────────────────────────────────────────────
try
{
    Console.WriteLine("[Program] Starting monitoring loop...");
    var monitor = serviceProvider.GetRequiredService<ISystemMonitorService>();
    await monitor.StartMonitoringAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("[Program] Monitor stopped gracefully.");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[Program] Fatal error: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}
finally
{
    Console.WriteLine("[Program] Disposing services...");
    serviceProvider.Dispose();
    Console.WriteLine("[Program] Shutdown complete.");
}