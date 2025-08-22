using Entities.Models;

namespace DataAccess.Contract;

public interface ILogFileDataService
{
    Task ProcessLogFileAsync(string filePath, TransferDirection direction);
}