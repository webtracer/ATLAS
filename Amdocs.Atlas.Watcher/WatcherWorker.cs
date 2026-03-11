using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class WatcherWorker : BackgroundService
{
    private readonly ILogger<WatcherWorker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public WatcherWorker(
        ILogger<WatcherWorker> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _config.GetValue("Watcher:IntervalSeconds", 15);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var primary = await ProbeAsync("Primary", stoppingToken);
                var backup  = await ProbeAsync("Backup", stoppingToken);

                var preferred =
                    primary.IsFullyHealthy ? "Primary" :
                    backup.IsFullyHealthy  ? "Backup"  :
                    "None";

                _logger.LogInformation("ATLAS Preferred={Preferred}", preferred);

                _logger.LogInformation(
                    "Primary: API={Api} WEB={Web}",
                    primary.ApiOk ? "UP" : "DOWN",
                    primary.WebOk ? "UP" : "DOWN"
                );

                _logger.LogInformation(
                    "Backup : API={Api} WEB={Web}",
                    backup.ApiOk ? "UP" : "DOWN",
                    backup.WebOk ? "UP" : "DOWN"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Watcher loop failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task<ProbeResult> ProbeAsync(string which, CancellationToken ct)
    {
        var apiUrl = _config[$"Watcher:{which}:ApiHealthUrl"];
        var webUrl = _config[$"Watcher:{which}:WebHealthUrl"];

        if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(webUrl))
            throw new InvalidOperationException($"Watcher URLs missing for {which}");

        // Create a client for this run and apply a short timeout so we never “hang”
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(3);

        var apiOk = await IsHealthyAsync(client, apiUrl, ct);
        var webOk = await IsHealthyAsync(client, webUrl, ct);

        return new ProbeResult(apiOk, webOk);
    }

    private static async Task<bool> IsHealthyAsync(HttpClient client, string url, CancellationToken ct)
    {
        try
        {
            using var resp = await client.GetAsync(url, ct);
            return resp.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    private readonly record struct ProbeResult(bool ApiOk, bool WebOk)
    {
        public bool IsFullyHealthy => ApiOk && WebOk;
    }
}
