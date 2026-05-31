using System.Diagnostics;
using SystemMonitorApp1.Interfaces;

namespace SystemMonitorApp1.Platform;

public class WindowsMetricsProvider : IPlatformMetricsProvider
{
    public string PlatformName => "Windows";

    public async Task<double> GetCpuUsagePercentAsync()
    {
        try
        {
            var processes = Process.GetProcesses();
            var totalCpuBefore = SumProcessorTime(processes);
            DisposeAll(processes);

            await Task.Delay(500);

            processes = Process.GetProcesses();
            var totalCpuAfter = SumProcessorTime(processes);
            DisposeAll(processes);

            var elapsed = 500.0;
            var cpuUsed = totalCpuAfter - totalCpuBefore;
            var cpuCount = Environment.ProcessorCount;

            return Math.Round(Math.Min(100, cpuUsed / (elapsed * cpuCount) * 100), 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WindowsMetricsProvider] CPU measurement failed: {ex.Message}");
            return 0;
        }
    }

    public Task<(long UsedMb, long TotalMb)> GetPrivateRamUsageAsync()
    {
        try
        {
            var usedMb = PrivateRamReader.GetPrivateRamUsedBytes() / 1024 / 1024;
            var totalMb = GetTotalPhysicalRamMb();

            Console.WriteLine($"[WindowsMetricsProvider] Private RAM: {usedMb}MB used / {totalMb}MB total");
            return Task.FromResult((usedMb, totalMb));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WindowsMetricsProvider] Private RAM measurement failed: {ex.Message}");
            return Task.FromResult((0L, 0L));
        }
    }

    public (long UsedMb, long TotalMb) GetDiskUsage() => DiskMetricsReader.Read();

    private static long GetTotalPhysicalRamMb()
    {
        var psi = new ProcessStartInfo("wmic", "OS get TotalVisibleMemorySize /Value")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi)!;
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        foreach (var line in output.Split('\n'))
        {
            if (line.StartsWith("TotalVisibleMemorySize=") &&
                long.TryParse(line.Split('=')[1].Trim(), out var totalKb))
            {
                return totalKb / 1024;
            }
        }

        return 0;
    }

    private static double SumProcessorTime(Process[] processes)
    {
        double total = 0;
        foreach (var process in processes)
        {
            try
            {
                total += process.TotalProcessorTime.TotalMilliseconds;
            }
            catch (Exception)
            {
                // Skip processes that exited or are inaccessible.
            }
            finally
            {
                process.Dispose();
            }
        }

        return total;
    }

    private static void DisposeAll(Process[] processes)
    {
        foreach (var process in processes)
            process.Dispose();
    }
}
