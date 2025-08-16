namespace Worker;

public class LogIngestOptions
{
    public string LogDirectory { get; set; } = default!;
    public string UploadPattern { get; set; } = "uploads_*.log";
    public string DownloadPattern { get; set; } = "downloads_*.log";
}
