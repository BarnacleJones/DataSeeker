namespace Entities.Models;

public class Folder
{
    public int FolderId { get; set; }
    public string Name { get; set; } = null!;

    public int? ParentId { get; set; }
    public Folder? Parent { get; set; }

    public ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
    public ICollection<FileEntry> Files { get; set; } = new List<FileEntry>();
}
