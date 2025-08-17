using System.Globalization;
using System.Text.RegularExpressions;
using Entities.Models;

namespace DataAccess.Service;

public class FileReaderToDataService : IFileReaderToDataService
{
    private readonly DataSeekerDbContext _context;

    public FileReaderToDataService(DataSeekerDbContext context)
    {
        _context = context;
    }

    public async Task ProcessUploadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var uploadFile = new UploadFile
        {
            UploadFileName = Path.GetFileName(filePath),
            UploadLines = new List<UploadLine>()
        };

        using var stream = new StreamReader(filePath);
        while (await stream.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var match = UploadFileRegex().Match(line);

            if (!match.Success) continue;

            var timestamp = match.Groups["timestamp"].Value;
            var user = match.Groups["user"].Value;
            var ip = match.Groups["ip"].Value;
            var filepath = match.Groups["filepath"].Value;

            var parsed = DateTime.TryParseExact(
                timestamp,
                "MM/dd/yy HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate
            );

            var uploadLine = new UploadLine
            {
                UploadDate = parsed ? DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc) : DateTime.UtcNow,
                User = user,
                IpAddress = ip,
                FileName = filepath,
                UploadFile = uploadFile
            };

            uploadFile.UploadLines.Add(uploadLine);
        }

        _context.UploadFiles.Add(uploadFile);
        await _context.SaveChangesAsync();
    }

    public async Task ProcessDownloadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var downloadFile = new DownloadFile
        {
            DownloadFileName = Path.GetFileName(filePath),
            DownloadLines = new List<DownloadLine>()
        };

        using var stream = new StreamReader(filePath);
        while (await stream.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var match = DownloadFileRegex().Match(line);

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

            var downloadLine = new DownloadLine
            {
                DownloadDate = parsed ? parsedDate : DateTime.UtcNow,
                User = user,
                FileName = filepath,
                DownloadFile = downloadFile
            };

            downloadFile.DownloadLines.Add(downloadLine);
        }

        _context.DownloadFiles.Add(downloadFile);
        await _context.SaveChangesAsync();
    }

    private Regex UploadFileRegex()
    {
        var regex = 
        @"^(?<timestamp>\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) Upload (started|finished): user (?<user>.*?), IP address \('(?<ip>[\d\.]+)', \d+\), file (?<filepath>.+)$";
        return new Regex(regex, RegexOptions.Compiled);
        
    }
    private Regex DownloadFileRegex()
    {
        var regex = @"^(?<timestamp>\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) Download (started|finished): user (?<user>.*?), file (?<filepath>.+)$";
        return new Regex(regex, RegexOptions.Compiled);
    }
}