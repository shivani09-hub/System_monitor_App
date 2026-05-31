using SystemMonitorApp1.Models;

namespace SystemMonitorApp1.Interfaces;

/// <summary>
/// Core monitoring service contract.
/// </summary>
public interface ISystemMonitorService
{
    /// <summary>Collects and returns current system metrics.</summary>
    Task<SystemMetrics> GetMetricsAsync();

    /// <summary>Starts the continuous monitoring loop until cancellation.</summary>
    Task StartMonitoringAsync(CancellationToken cancellationToken);
}