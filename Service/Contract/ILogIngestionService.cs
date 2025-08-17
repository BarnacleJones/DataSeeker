namespace Service.Contract;

public interface ILogIngestionService
{
    Task IngestLogsAsync();
}