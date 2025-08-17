using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Contract;

namespace Worker;

public class LogIngester : BackgroundService
{
    private readonly ILogIngestionService _logIngestionService;
    private readonly ILogger<LogIngester> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public LogIngester(ILogIngestionService logIngestionService, ILogger<LogIngester> logger)
    {
        _logIngestionService = logIngestionService;
        _logger = logger;
    }

    public async Task IngestLogs()
    {
        await _logIngestionService.IngestLogsAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _logIngestionService.IngestLogsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing logs");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}