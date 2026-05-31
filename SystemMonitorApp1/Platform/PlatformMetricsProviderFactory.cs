using System.Runtime.InteropServices;
using SystemMonitorApp1.Interfaces;

namespace SystemMonitorApp1.Platform;

public static class PlatformMetricsProviderFactory
{
    public static IPlatformMetricsProvider Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsMetricsProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxMetricsProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacMetricsProvider();

        throw new PlatformNotSupportedException(
            "System monitoring is not supported on this operating system.");
    }
}
