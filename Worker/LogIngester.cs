using DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Worker;

public class LogIngester : BackgroundService
{
    private readonly ILogger<LogIngester> _logger;
    private readonly IServiceProvider _services;
    private readonly IOptions<LogIngestOptions> _options;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public LogIngester(
        ILogger<LogIngester> logger,
        IServiceProvider services,
        IOptions<LogIngestOptions> options)
    {
        _logger = logger;
        _services = services;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessLogsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing logs");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessLogsAsync()
    {
        var dir = _options.Value.LogDirectory;
        var processedDir = Path.Combine(dir, "processed");
        Directory.CreateDirectory(processedDir);

        var uploadFiles = Directory.GetFiles(dir, _options.Value.UploadPattern);
        var downloadFiles = Directory.GetFiles(dir, _options.Value.DownloadPattern);

        using var scope = _services.CreateScope();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadService>();

        foreach (var filePath in uploadFiles)
        {
            try
            {
                await uploadService.ProcessUploadAsync(filePath);
                MoveToProcessed(filePath, processedDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing upload file {File}", filePath);
            }
        }

        foreach (var filePath in downloadFiles)
        {
            try
            {
                await uploadService.ProcessDownloadAsync(filePath);
                MoveToProcessed(filePath, processedDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing download file {File}", filePath);
            }
        }
    }

    private void MoveToProcessed(string originalPath, string processedDir)
    {
        var fileName = Path.GetFileName(originalPath);
        var destinationPath = Path.Combine(processedDir, fileName);

        if (File.Exists(destinationPath))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            destinationPath = Path.Combine(processedDir, $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}");
        }

        File.Move(originalPath, destinationPath);
    }
}
