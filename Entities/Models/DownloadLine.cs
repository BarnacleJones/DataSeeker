namespace DataSeek.Web.DataModels;

public class DownloadLine
{
    public int Id { get; set; }
    public DateTime DownloadDate { get; set; }
    public string? User { get; set; }
    public string? FileName { get; set; }

    public int DownloadFileId { get; set; }
    public DownloadFile? DownloadFile { get; set; }
}