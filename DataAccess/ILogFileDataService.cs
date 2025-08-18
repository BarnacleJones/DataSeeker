using Entities.Models;

namespace DataAccess;

public interface ILogFileDataService
{
    Task ProcessLogFileAsync(string filePath, TransferDirection direction);
}