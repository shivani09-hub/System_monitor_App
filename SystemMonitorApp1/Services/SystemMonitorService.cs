using Microsoft.Extensions.Configuration;
using SystemMonitorApp1.Interfaces;
using SystemMonitorApp1.Models;

namespace SystemMonitorApp1.Services;

/// <summary>
/// Core monitoring service. Delegates metric collection to a platform provider
/// and runs all registered plugins after each cycle.
/// </summary>
public class SystemMonitorService : ISystemMonitorService
{
    private readonly IEnumerable<IMonitorPlugin> _plugins;
    private readonly IPlatformMetricsProvider _platformProvider;
    private readonly int _intervalSeconds;

    public SystemMonitorService(
        IEnumerable<IMonitorPlugin> plugins,
        IPlatformMetricsProvider platformProvider,
        IConfiguration configuration)
    {
        _plugins = plugins;
        _platformProvider = platformProvider;
        _intervalSeconds = configuration.GetValue<int>("MonitoringInterval", 5);
        Console.WriteLine(
            $"[SystemMonitorService] Initialized on {_platformProvider.PlatformName}. " +
            $"Interval: {_intervalSeconds}s, Plugins loaded: {_plugins.Count()}");
    }

    public async Task<SystemMetrics> GetMetricsAsync()
    {
        Console.WriteLine("[SystemMonitorService] Collecting metrics...");

        var (ramUsedMb, ramTotalMb) = await _platformProvider.GetPrivateRamUsageAsync();
        var (diskUsedMb, diskTotalMb) = _platformProvider.GetDiskUsage();

        var metrics = new SystemMetrics
        {
            CpuUsagePercent = await _platformProvider.GetCpuUsagePercentAsync(),
            RamUsedMb = ramUsedMb,
            RamTotalMb = ramTotalMb,
            DiskUsedMb = diskUsedMb,
            DiskTotalMb = diskTotalMb,
            Timestamp = DateTime.UtcNow
        };

        Console.WriteLine(
            $"[SystemMonitorService] Metrics collected: CPU={metrics.CpuUsagePercent}%, " +
            $"Private RAM={metrics.RamUsedMb}MB/{metrics.RamTotalMb}MB, " +
            $"Disk={metrics.DiskUsedMb}MB/{metrics.DiskTotalMb}MB");

        return metrics;
    }

    public async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[SystemMonitorService] Monitoring started. Press Ctrl+C to stop.\n");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var metrics = await GetMetricsAsync();
                DisplayMetrics(metrics);
                await RunPluginsAsync(metrics);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[SystemMonitorService] Monitoring error: {ex.Message}");
                Console.ResetColor();
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), cancellationToken)
                      .ContinueWith(_ => { }, CancellationToken.None);
        }

        Console.WriteLine("[SystemMonitorService] Monitoring stopped.");
    }

    private static void DisplayMetrics(SystemMetrics metrics)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("===== SYSTEM MONITOR =====");
        Console.ResetColor();
        Console.WriteLine();

        Console.Write("CPU Usage  : ");
        SetUsageColor(metrics.CpuUsagePercent, 60, 85);
        Console.WriteLine($"{metrics.CpuUsagePercent:F1}%");
        Console.ResetColor();

        double ramPercent = metrics.RamTotalMb > 0
            ? (double)metrics.RamUsedMb / metrics.RamTotalMb * 100 : 0;
        Console.Write("Private RAM: ");
        SetUsageColor(ramPercent, 70, 90);
        Console.WriteLine($"{metrics.RamUsedMb:N0} MB / {metrics.RamTotalMb:N0} MB");
        Console.ResetColor();

        double diskPercent = metrics.DiskTotalMb > 0
            ? (double)metrics.DiskUsedMb / metrics.DiskTotalMb * 100 : 0;
        Console.Write("Disk Usage : ");
        SetUsageColor(diskPercent, 75, 90);
        Console.WriteLine($"{metrics.DiskUsedMb:N0} MB / {metrics.DiskTotalMb:N0} MB");
        Console.ResetColor();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==========================");
        Console.ResetColor();
        Console.WriteLine($"Last updated: {metrics.Timestamp:HH:mm:ss} UTC");
    }

    private static void SetUsageColor(double percent, double warnThreshold, double critThreshold)
    {
        Console.ForegroundColor = percent >= critThreshold
            ? ConsoleColor.Red
            : percent >= warnThreshold
                ? ConsoleColor.Yellow
                : ConsoleColor.Green;
    }

    private async Task RunPluginsAsync(SystemMetrics metrics)
    {
        Console.WriteLine($"[SystemMonitorService] Running {_plugins.Count()} plugin(s)...");
        foreach (var plugin in _plugins)
        {
            try
            {
                Console.WriteLine($"[SystemMonitorService] Executing plugin: {plugin.Name}");
                await plugin.ExecuteAsync(metrics);
                Console.WriteLine($"[SystemMonitorService] Plugin '{plugin.Name}' completed successfully.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[SystemMonitorService] Plugin '{plugin.Name}' failed: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
