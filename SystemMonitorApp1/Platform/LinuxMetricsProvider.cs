using System.Diagnostics;
using SystemMonitorApp1.Interfaces;

namespace SystemMonitorApp1.Platform;

public class LinuxMetricsProvider : IPlatformMetricsProvider
{
    public string PlatformName => "Linux";

    public async Task<double> GetCpuUsagePercentAsync()
    {
        try
        {
            var stat1 = await ReadProcStatAsync();
            await Task.Delay(500);
            var stat2 = await ReadProcStatAsync();

            var idle1 = stat1[3] + stat1[4];
            var idle2 = stat2[3] + stat2[4];
            var totalDiff = stat2.Sum() - stat1.Sum();
            var idleDiff = idle2 - idle1;

            if (totalDiff == 0)
                return 0;

            return Math.Round((1.0 - (double)idleDiff / totalDiff) * 100, 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LinuxMetricsProvider] CPU measurement failed: {ex.Message}");
            return 0;
        }
    }

    public Task<(long UsedMb, long TotalMb)> GetPrivateRamUsageAsync()
    {
        try
        {
            var usedMb = GetPrivateRamUsedMb();
            var totalMb = ReadMemInfoValueKb("MemTotal:") / 1024;

            Console.WriteLine($"[LinuxMetricsProvider] Private RAM: {usedMb}MB used / {totalMb}MB total");
            return Task.FromResult((usedMb, totalMb));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LinuxMetricsProvider] Private RAM measurement failed: {ex.Message}");
            return Task.FromResult((0L, 0L));
        }
    }

    public (long UsedMb, long TotalMb) GetDiskUsage() => DiskMetricsReader.Read();

    private static long GetPrivateRamUsedMb()
    {
        long privateKb = 0;

        foreach (var dir in Directory.EnumerateDirectories("/proc"))
        {
            if (!int.TryParse(Path.GetFileName(dir), out _))
                continue;

            var rollupPath = Path.Combine(dir, "smaps_rollup");
            if (!File.Exists(rollupPath))
                continue;

            foreach (var line in File.ReadLines(rollupPath))
            {
                if (line.StartsWith("Private_Clean:", StringComparison.Ordinal) ||
                    line.StartsWith("Private_Dirty:", StringComparison.Ordinal))
                {
                    privateKb += ParseMemInfoKb(line);
                }
            }
        }

        if (privateKb > 0)
            return privateKb / 1024;

        return PrivateRamReader.GetPrivateRamUsedBytes() / 1024 / 1024;
    }

    private static long ReadMemInfoValueKb(string key)
    {
        foreach (var line in File.ReadLines("/proc/meminfo"))
        {
            if (line.StartsWith(key, StringComparison.Ordinal))
                return ParseMemInfoKb(line);
        }

        return 0;
    }

    private static async Task<long[]> ReadProcStatAsync()
    {
        var lines = await File.ReadAllLinesAsync("/proc/stat");
        var cpuLine = lines.First(l => l.StartsWith("cpu ", StringComparison.Ordinal));
        var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1);
        return parts.Select(long.Parse).ToArray();
    }

    private static long ParseMemInfoKb(string line)
    {
        var parts = line.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            return 0;

        var valuePart = parts[1].Replace("kB", "", StringComparison.Ordinal).Trim();
        return long.TryParse(valuePart, out var val) ? val : 0;
    }
}
