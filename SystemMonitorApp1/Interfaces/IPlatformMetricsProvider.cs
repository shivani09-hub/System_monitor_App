using SystemMonitorApp1.Models;

namespace SystemMonitorApp1.Interfaces;

/// <summary>
/// Platform-specific strategy for collecting CPU, private RAM, and disk metrics.
/// Each OS implements this interface so core monitoring logic stays unchanged.
/// </summary>
public interface IPlatformMetricsProvider
{
    string PlatformName { get; }

    Task<double> GetCpuUsagePercentAsync();

    /// <summary>Returns system private RAM used and total physical RAM, both in MB.</summary>
    Task<(long UsedMb, long TotalMb)> GetPrivateRamUsageAsync();

    /// <summary>Returns disk space used and total capacity, both in MB.</summary>
    (long UsedMb, long TotalMb) GetDiskUsage();
}
