using System.Diagnostics;
using System.Text.RegularExpressions;
using SystemMonitorApp1.Interfaces;

namespace SystemMonitorApp1.Platform;

public class MacMetricsProvider : IPlatformMetricsProvider
{
    public string PlatformName => "macOS";

    public async Task<double> GetCpuUsagePercentAsync()
    {
        try
        {
            var psi = new ProcessStartInfo("sh", "-c \"top -l 1 | grep 'CPU usage'\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(psi)!;
            var output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();

            var idleMatch = Regex.Match(output, @"(\d+\.?\d*)% idle");
            if (idleMatch.Success && double.TryParse(idleMatch.Groups[1].Value, out var idle))
                return Math.Round(100 - idle, 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MacMetricsProvider] CPU measurement failed: {ex.Message}");
        }

        return 0;
    }

    public Task<(long UsedMb, long TotalMb)> GetPrivateRamUsageAsync()
    {
        try
        {
            var usedMb = PrivateRamReader.GetPrivateRamUsedBytes() / 1024 / 1024;
            var totalMb = GetTotalPhysicalRamMb();

            Console.WriteLine($"[MacMetricsProvider] Private RAM: {usedMb}MB used / {totalMb}MB total");
            return Task.FromResult((usedMb, totalMb));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MacMetricsProvider] Private RAM measurement failed: {ex.Message}");
            return Task.FromResult((0L, 0L));
        }
    }

    public (long UsedMb, long TotalMb) GetDiskUsage() => DiskMetricsReader.Read();

    private static long GetTotalPhysicalRamMb()
    {
        var psi = new ProcessStartInfo("sysctl", "-n hw.memsize")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi)!;
        var output = proc.StandardOutput.ReadToEnd().Trim();
        proc.WaitForExit();

        return long.TryParse(output, out var bytes) ? bytes / 1024 / 1024 : 0;
    }
}
