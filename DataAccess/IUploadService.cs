namespace DataAccess;

public interface IUploadService
{
    Task ProcessUploadAsync(string filePath);
    Task ProcessDownloadAsync(string filePath);
}