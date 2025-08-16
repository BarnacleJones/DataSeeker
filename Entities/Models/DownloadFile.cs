namespace Entities.Models;

public class DownloadFile
{
    public int DownloadFileId { get; set; }
    public string? DownloadFileName { get; set; }
    public List<DownloadLine> DownloadLines { get; set; } = new();
}