using System.Globalization;
using System.Text.RegularExpressions;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Service;

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
        using var stream = new StreamReader(filePath);
        while (await stream.ReadLineAsync() is { } logLine)
        {
            if (string.IsNullOrWhiteSpace(logLine)) continue;
            
            var finishedUploadLine = FinishedUploadLineRegex().Match(logLine);

            if (!finishedUploadLine.Success) continue;

            var uploadTimestamp = finishedUploadLine.Groups["timestamp"].Value;
            var userUploadedTo = finishedUploadLine.Groups["user"].Value;
            var localFullFilePath = finishedUploadLine.Groups["filepath"].Value;
            // var ip = match.Groups["ip"].Value; //dont care about ip at this stage, but its here
            
            var localFilePathPartsArray = localFullFilePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            
            var fileName = localFilePathPartsArray.Last();
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
        }
        _context.LogFiles.Add(newLogFileRecord);
        await _context.SaveChangesAsync();
        
        var logFileAndLines = _context.LogFiles
            .Include(x => x.LogLines)
            .FirstOrDefault(f => f.LogFileName == newLogFileRecord.LogFileName);

        if (logFileAndLines != null)
        {
            //once all log lines are stored, pass the log file lines
            //to associate the upload line to an uploadedFile record
            await AssociateLogFileLinesToUploadFilesAndLocalFolders(logFileAndLines.LogLines);
        }

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
            var downloadedFilePathPartsArray = filepath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            
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

    private async Task AssociateLogFileLinesToUploadFilesAndLocalFolders(List<LogLine> uploadLogLines)
    {
        foreach (var uploadLine in uploadLogLines)
        {
            var filePathForLocalFileUploaded = uploadLine.FullFilePath;
            if (!string.IsNullOrWhiteSpace(filePathForLocalFileUploaded))
            {
                await ProcessLocalFileFolderData(filePathForLocalFileUploaded, uploadLine.LogLineId);
            }
        }
    }

    private async Task ProcessLocalFileFolderData(string uploadedFilepath, int logLineId)
    {
        var folders = uploadedFilepath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        var fileName = folders.Last();
        LocalFolder? parent = null;

        // Walk through folder hierarchy
        for (int i = 0; i < folders.Length - 1; i++)
        {
            string folderName = folders[i];
            var folderQueryable = _context.LocalFolders.AsQueryable();

            if (parent != null)
            {
                var parent1 = parent;
                folderQueryable = folderQueryable.Where(x => x.ParentFolderId == parent1.LocalFolderId);
            }
            var localFolder = await folderQueryable.FirstOrDefaultAsync(f => f.Name == folderName);

            if (localFolder == null)
            {
                localFolder = new LocalFolder
                {
                    Name = folderName, 
                    ParentFolder = parent
                };
                _context.LocalFolders.Add(localFolder);
                
                await _context.SaveChangesAsync(); // Save to get FolderId
            }

            parent = localFolder;
        }

        //create root if no parent exists
        if (parent == null)
        {
            parent = await _context.LocalFolders.FirstOrDefaultAsync(f => f.Name == "ROOT");
            if (parent == null)
            {
                parent = new LocalFolder { Name = "ROOT" };
                _context.LocalFolders.Add(parent);
                await _context.SaveChangesAsync();
            }
        }
        
        var uploadedFile = new UploadedFile()
        {
            FileName = fileName,
            ContainingFolder = parent
        };

        _context.UploadedFiles.Add(uploadedFile);
        await _context.SaveChangesAsync();

        //Associate log line with uploaded file
        var logLine = await _context.LogLines.Where(x => x.LogLineId == logLineId).FirstOrDefaultAsync();
        
        if (logLine != null) logLine.UploadedFile = uploadedFile;
    }
   
    private Regex FinishedUploadLineRegex()
    {
        var regex = 
        @"^(?<timestamp>\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) Upload finished: user (?<user>.*?), IP address \('(?<ip>[\d\.]+)', \d+\), file (?<filepath>.+)$";
        return new Regex(regex, RegexOptions.Compiled);
        
    }
    private Regex FinishedDownloadLineRegex()
    {
        var regex = @"^(?<timestamp>\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) Download finished: user (?<user>.*?), file (?<filepath>.+)$";
        return new Regex(regex, RegexOptions.Compiled);
    }

}