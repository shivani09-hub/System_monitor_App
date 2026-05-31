using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using SystemMonitorApp1.Models;
using SystemMonitorApp1.Interfaces;

namespace SystemMonitorApp1.Plugins;

/// <summary>
/// Plugin that sends system metrics as JSON to a configurable REST API
/// endpoint via HTTP POST after every monitoring cycle.
/// </summary>
public class ApiPlugin : IMonitorPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiUrl;

    public string Name => "ApiPlugin";

    public ApiPlugin(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("MonitorApi");
        _apiUrl = configuration["ApiUrl"];
        Console.WriteLine($"[ApiPlugin] Initialized. API URL: {_apiUrl}");
    }

    public async Task ExecuteAsync(SystemMetrics metrics)
    {
        if (string.IsNullOrWhiteSpace(_apiUrl) || _apiUrl.Contains("example.com"))
        {
            Console.WriteLine("[ApiPlugin] Skipping - API URL is not configured.");
            return;
        }

        var payload = new
        {
            cpu = metrics.CpuUsagePercent,
            ram_used = metrics.RamUsedMb,
            disk_used = metrics.DiskUsedMb
        };

        Console.WriteLine($"[ApiPlugin] Sending POST to {_apiUrl}...");
        Console.WriteLine($"[ApiPlugin] Payload: cpu={payload.cpu}, ram_used={payload.ram_used}, disk_used={payload.disk_used}");

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _httpClient.PostAsJsonAsync(_apiUrl, payload, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ApiPlugin] API returned error: {(int)response.StatusCode} {response.ReasonPhrase}");
                throw new HttpRequestException($"API returned: {(int)response.StatusCode}");
            }

            Console.WriteLine($"[ApiPlugin] POST successful. Status: {(int)response.StatusCode}");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[ApiPlugin] Request timed out after 10 seconds.");
            throw new TimeoutException("API request timed out.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ApiPlugin] HTTP error: {ex.Message}");
            throw new InvalidOperationException($"API POST failed: {ex.Message}", ex);
        }
    }
}