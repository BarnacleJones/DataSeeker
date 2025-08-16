namespace DataSeek.Web.DataModels;

public class UploadFile
{
    public int UploadFileId { get; set; }
    public string? UploadFileName { get; set; }
    public List<UploadLine> UploadLines { get; set; }
}