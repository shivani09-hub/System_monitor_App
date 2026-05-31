namespace SystemMonitorApp1.Platform;

/// <summary>Cross-platform disk metrics via DriveInfo.</summary>
internal static class DiskMetricsReader
{
    public static (long UsedMb, long TotalMb) Read()
    {
        var drive = DriveInfo.GetDrives()
            .FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);

        if (drive == null)
            return (0, 0);

        var totalMb = drive.TotalSize / 1024 / 1024;
        var usedMb = (drive.TotalSize - drive.AvailableFreeSpace) / 1024 / 1024;
        return (usedMb, totalMb);
    }
}
