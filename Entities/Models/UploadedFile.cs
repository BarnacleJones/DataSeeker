namespace Entities.Models;

public class UploadedFile
{
    public int UploadedFileId { get; set; }
    public string FileName { get; set; } = null!;
    public int? LocalFolderId { get; set; }
    public LocalFolder? ContainingFolder { get; set; } = null!; //nullable - can exist at root level
    public List<LogLine> LogLines { get; set; } = new List<LogLine>();
}
