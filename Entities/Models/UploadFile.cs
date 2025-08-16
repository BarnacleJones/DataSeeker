namespace Entities.Models;

public class UploadFile
{
    public int UploadFileId { get; set; }
    public string? UploadFileName { get; set; }
    public List<UploadLine> UploadLines { get; set; }
}