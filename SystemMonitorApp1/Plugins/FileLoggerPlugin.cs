using SystemMonitorApp1.Models;
using SystemMonitorApp1.Interfaces;

namespace SystemMonitorApp1.Plugins;

/// <summary>
/// Plugin that appends system metrics to a local log file (systemlogs.txt)
/// after every monitoring cycle.
/// </summary>
public class FileLoggerPlugin : IMonitorPlugin
{
    private const string LogFileName = "systemlogs.txt";
    private readonly SemaphoreSlim _lock = new(1, 1);

    public string Name => "FileLogger";

    public async Task ExecuteAsync(SystemMetrics metrics)
    {
        Console.WriteLine($"[FileLoggerPlugin] Writing metrics to '{LogFileName}'...");

        var logEntry = FormatLogEntry(metrics);

        await _lock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(LogFileName, logEntry);
            Console.WriteLine($"[FileLoggerPlugin] Successfully written to '{LogFileName}'.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[FileLoggerPlugin] File write error: {ex.Message}");
            throw new InvalidOperationException($"Failed to write log file '{LogFileName}': {ex.Message}", ex);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string FormatLogEntry(SystemMetrics metrics)
    {
        return $"[{metrics.Timestamp:yyyy-MM-dd HH:mm:ss} UTC] " +
               $"CPU: {metrics.CpuUsagePercent:F1}% | " +
               $"Private RAM: {metrics.RamUsedMb:N0}/{metrics.RamTotalMb:N0} MB | " +
               $"Disk: {metrics.DiskUsedMb:N0}/{metrics.DiskTotalMb:N0} MB{Environment.NewLine}";
    }
}