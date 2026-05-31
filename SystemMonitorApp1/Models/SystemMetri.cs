namespace SystemMonitorApp1.Models;

public class SystemMetrics
{
    public double CpuUsagePercent { get; set; }
    public long RamUsedMb { get; set; }
    public long RamTotalMb { get; set; }
    public long DiskUsedMb { get; set; }
    public long DiskTotalMb { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
