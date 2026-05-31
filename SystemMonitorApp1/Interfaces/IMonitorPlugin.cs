using SystemMonitorApp1.Models;

namespace SystemMonitorApp1.Interfaces;

/// <summary>
/// Plugin interface - implement this to add new monitoring integrations
/// without changing any core code.
/// </summary>
public interface IMonitorPlugin
{
    /// <summary>Plugin name used in logs and error messages.</summary>
    string Name { get; }

    /// <summary>Called after every monitoring cycle with the latest metrics.</summary>
    Task ExecuteAsync(SystemMetrics metrics);
}