using System.Diagnostics;

namespace SystemMonitorApp1.Platform;

/// <summary>Sums private memory across all accessible processes.</summary>
internal static class PrivateRamReader
{
    public static long GetPrivateRamUsedBytes()
    {
        long totalBytes = 0;

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                totalBytes += process.PrivateMemorySize64;
            }
            catch (Exception)
            {
                // Access denied for some system processes — skip them.
            }
            finally
            {
                process.Dispose();
            }
        }

        return totalBytes;
    }
}
