namespace Entities.Models;

public class FileEntry
{
    public int FileEntryId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileType { get; set; } = null!;

    public int FolderId { get; set; }
    public Folder Folder { get; set; } = null!;
}
