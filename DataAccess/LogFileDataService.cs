using System.Globalization;
using System.Text.RegularExpressions;
using DataAccess.Contract;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class LogFileDataService : ILogFileDataService
{
    private readonly DataSeekerDbContext _context;
    public LogFileDataService(DataSeekerDbContext context)
    {
        _context = context;
    }

    public Task ProcessLogFileAsync(string filePath, TransferDirection direction)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        
        var newLogFileRecord = new LogFile
        {
            LogFileName = Path.GetFileName(filePath),
            TransferDirection = TransferDirection.Upload,
            LogLines = new List<LogLine>()
        };
        switch (direction)
        {
            case TransferDirection.Download:
                return PersistFinishedDownloadLogFile(filePath, newLogFileRecord);
            case TransferDirection.Upload:
                return PersistFinishedUploadLogFile(filePath, newLogFileRecord);
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
    private async Task PersistFinishedUploadLogFile(string filePath, LogFile newLogFileRecord)
    {
        var folderCache = await _context.LocalFolders
            .Include(f => f.ParentFolder)
            .ToListAsync();

        var root = folderCache.FirstOrDefault(f => f.Name == "ROOT" && f.ParentFolderId == null);
        if (root == null)
        {
            root = new LocalFolder { Name = "ROOT" };
            _context.LocalFolders.Add(root);
            await _context.SaveChangesAsync();
            folderCache.Add(root);
        }

        var folderDict = folderCache.ToDictionary(
            f => BuildPath(f),
            f => f
        );

        var newFolders = new List<LocalFolder>();
        var newFiles = new List<UploadedFile>();

        using var stream = new StreamReader(filePath);
        while (await stream.ReadLineAsync() is { } logLine)
        {
            if (string.IsNullOrWhiteSpace(logLine)) continue;

            var match = FinishedUploadLineRegex().Match(logLine);
            if (!match.Success) continue;

            var uploadTimestamp = match.Groups["timestamp"].Value;
            var userUploadedTo = match.Groups["user"].Value;
            var localFullFilePath = match.Groups["filepath"].Value;

            var pathParts = localFullFilePath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var fileName = pathParts.Last();
            var fileType = Path.GetExtension(fileName).TrimStart('.').ToLower();

            var parsed = DateTime.TryParseExact(
                uploadTimestamp,
                "MM/dd/yy HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var uploadTimestampParsed
            );

            var logLineForUpload = new LogLine
            {
                DateTime = parsed ? DateTime.SpecifyKind(uploadTimestampParsed, DateTimeKind.Utc) : null,
                User = userUploadedTo,
                FullFilePath = localFullFilePath,
                FileType = fileType
            };

            newLogFileRecord.LogLines.Add(logLineForUpload);

            //Build folder heirarchy todo split out into method
            LocalFolder parent = root;
            string parentPath = BuildPath(root);

            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                var folderName = pathParts[i];
                var currentPath = $"{parentPath}{Path.DirectorySeparatorChar}{folderName}";

                if (!folderDict.TryGetValue(currentPath, out var folder))
                {
                    folder = new LocalFolder
                    {
                        Name = folderName,
                        ParentFolder = parent
                    };
                    folderDict[currentPath] = folder;
                    newFolders.Add(folder);
                }

                parent = folder;
                parentPath = currentPath;
            }

            var uploadedFile = new UploadedFile
            {
                FileName = fileName,
                ContainingFolder = parent
            };

            newFiles.Add(uploadedFile);
            logLineForUpload.UploadedFile = uploadedFile;
        }

        _context.LocalFolders.AddRange(newFolders);
        _context.UploadedFiles.AddRange(newFiles);
        _context.LogFiles.Add(newLogFileRecord);

        await _context.SaveChangesAsync();
    }
    private async Task PersistFinishedDownloadLogFile(string filePath, LogFile logFile)
    {
        using var stream = new StreamReader(filePath);
        while (await stream.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var match = FinishedDownloadLineRegex().Match(line);

            if (!match.Success) continue;

            var timestamp = match.Groups["timestamp"].Value;
            var user = match.Groups["user"].Value;
            var filepath = match.Groups["filepath"].Value;

            var parsed = DateTime.TryParseExact(
                timestamp,
                "MM/dd/yy HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedDate
            );
            var downloadedFilePathPartsArray = filepath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            
            var fileName = downloadedFilePathPartsArray.Last();
            var fileType = Path.GetExtension(fileName).TrimStart('.').ToLower();

            var logLine = new LogLine
            {
                DateTime = parsed ? parsedDate : DateTime.UtcNow,
                User = user,
                FullFilePath = filepath,
                FileType = fileType
            };

            logFile.LogLines.Add(logLine);
        }

        _context.LogFiles.Add(logFile);
        await _context.SaveChangesAsync();
    }
    private Regex FinishedUploadLineRegex()
    {
        var regex =
            @"^(?<timestamp>\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) Upload finished: user (?<user>.*?), IP address (?:(?:\('(?<ip>[\d\.]+)', \d+\))|None), file (?<filepath>.+)$";
        return new Regex(regex, RegexOptions.Compiled);
    }
    private Regex FinishedDownloadLineRegex()
    {
        var regex = @"^(?<timestamp>\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) Download finished: user (?<user>.*?), file (?<filepath>.+)$";
        return new Regex(regex, RegexOptions.Compiled);
    }
    private string BuildPath(LocalFolder folder)
    {
        return folder.ParentFolder == null
            ? folder.Name
            : $"{BuildPath(folder.ParentFolder)}{Path.DirectorySeparatorChar}{folder.Name}";
    }
}