using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataAccess;
using Service.Contract;

namespace Service
{
    public class LogIngestionService : ILogIngestionService
    {
        private readonly ILogger<LogIngestionService> _logger;
        private readonly IOptions<LogIngestOptions> _options;
        private readonly IFileReaderToDataService _fileReaderToDataService;

        public LogIngestionService(
            ILogger<LogIngestionService> logger,
            IOptions<LogIngestOptions> options, IFileReaderToDataService fileReaderToDataService)
        {
            _logger = logger;
            _options = options;
            _fileReaderToDataService = fileReaderToDataService;
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
                    await _fileReaderToDataService.ProcessUploadAsync(filePath);
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
                    await _fileReaderToDataService.ProcessDownloadAsync(filePath);
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
