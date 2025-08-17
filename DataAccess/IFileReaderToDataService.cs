namespace DataAccess;

public interface IFileReaderToDataService
{
    Task ProcessUploadAsync(string filePath);
    Task ProcessDownloadAsync(string filePath);
}