using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataAccess;
using Entities.Models;
using Service.Contract;

namespace Service
{
    public class LogIngestionService : ILogIngestionService
    {
        private readonly ILogger<LogIngestionService> _logger;
        private readonly IOptions<LogIngestOptions> _options;
        private readonly ILogFileDataService _logFileDataService;

        public LogIngestionService(
            ILogger<LogIngestionService> logger,
            IOptions<LogIngestOptions> options, 
            ILogFileDataService logFileDataService)
        {
            _logger = logger;
            _options = options;
            _logFileDataService = logFileDataService;
        }

        public async Task IngestLogsAsync()
        {
            _logger.LogInformation("Log ingestion triggered.");

            var dir = _options.Value.LogDirectory;
            var processedDir = Path.Combine(dir, "processed");
            Directory.CreateDirectory(processedDir);

            var uploadFiles = Directory.GetFiles(dir, _options.Value.UploadPattern);
            var downloadFiles = Directory.GetFiles(dir, _options.Value.DownloadPattern);

            foreach (var filePath in uploadFiles)
            {
                try
                {
                    await _logFileDataService.ProcessLogFileAsync(filePath, TransferDirection.Upload);
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
                    await _logFileDataService.ProcessLogFileAsync(filePath, TransferDirection.Download);
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
}
