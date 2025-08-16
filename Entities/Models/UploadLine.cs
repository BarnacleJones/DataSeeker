namespace Entities.Models;

public class UploadLine
{
    public int Id { get; set; }
    public DateTime UploadDate { get; set; }
    public string? User  { get; set; }
    public string? IpAddress { get; set; }
    public string? FileName { get; set; }
    public int UploadFileId  { get; set; }
    public UploadFile? UploadFile { get; set; }
}