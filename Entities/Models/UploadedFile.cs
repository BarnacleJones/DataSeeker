namespace Entities.Models;

public class UploadedFile
{
    public int UploadedFileId { get; set; }
    public string FileName { get; set; } = null!;
    public int FolderId { get; set; }
    public LocalFolder ContainingFolder { get; set; } = null!;
    public List<LogLine> LogLines { get; set; } = new List<LogLine>();
}
