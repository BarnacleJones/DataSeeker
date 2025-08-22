namespace Entities.Models;

public class LocalFolder
{
    public int LocalFolderId { get; set; }
    public string Name { get; set; } = null!;

    public int? ParentFolderId { get; set; }
    public LocalFolder? ParentFolder { get; set; }

    public ICollection<LocalFolder> SubFolders { get; set; } = new List<LocalFolder>();
    public ICollection<UploadedFile> UploadedFiles { get; set; } = new List<UploadedFile>();
}
