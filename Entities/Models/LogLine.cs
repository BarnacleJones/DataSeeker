namespace Entities.Models;

public class LogLine
{
    public int LogLineId { get; set; }
    public DateTime? DateTime { get; set; } //either store nothing or datetime now if it cant parse...may as well be nothing
    public string? User { get; set; }
    public string FullFilePath { get; set; } = null!;
    public string FileType { get; set; } = null!;
    public int LogFileId { get; set; }
    public LogFile? LogFile { get; set; }
    
    public int? UploadedFileId { get; set; } //nullable - dont store additional data for download logs
    public UploadedFile? UploadedFile { get; set; }
}