namespace Entities.Models;

public class LogFile
{
    public int LogFileId { get; set; }
    public string? LogFileName { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public List<LogLine> LogLines { get; set; } = new();
}