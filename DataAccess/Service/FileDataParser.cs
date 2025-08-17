using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Service;

public class FileDataParser
{
    private readonly DataSeekerDbContext _context;

    public FileDataParser(DataSeekerDbContext context)
    {
        _context = context;
    }

    public void Parse()
    {
        var unprocessedUploadLinesDb = _context.UploadLines.Where(x => !x.ProcessedFilePaths).AsQueryable();
        
        var fullPathsAndFileIds = unprocessedUploadLinesDb.
            Select(x => new { FileId = x.UploadFileId, Path = x.FileName })
            .ToDictionary(x => x.FileId, y => y.Path);
        
        var uploadFileIdsThatHaveBeenProcessed = new List<int>(fullPathsAndFileIds.Count);
        
        foreach (var uploadLine in fullPathsAndFileIds)
        {
            var filePath = uploadLine.Value;
            if (filePath != null)
            {
                var idk = ProcessFileFolderData(filePath);
                uploadFileIdsThatHaveBeenProcessed.Add(uploadLine.Key);
            }
        }
        
        var uploadLinesToUpdate = _context.UploadLines.Where(x => uploadFileIdsThatHaveBeenProcessed.Contains(x.UploadFileId))
            .AsQueryable();
        uploadLinesToUpdate.ForEachAsync(x => x.ProcessedFilePaths = true);
        _context.SaveChanges();
        
    }

    private async Task<int> ProcessFileFolderData(string filePath)
    {
        var parts = filePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        Folder? parent = null;

        // Walk through folder hierarchy
        for (int i = 0; i < parts.Length - 1; i++)
        {
            string folderName = parts[i];
            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Name == folderName && parent != null && f.ParentId == parent.FolderId);

            if (folder == null)
            {
                folder = new Folder { Name = folderName, Parent = parent };
                _context.Folders.Add(folder);
                await _context.SaveChangesAsync(); // Save to get FolderId
            }

            parent = folder;
        }

        // Final part is the file
        var fileName = parts.Last();
        var fileType = Path.GetExtension(fileName).TrimStart('.').ToLower();

        var fileEntry = new FileEntry
        {
            FileName = fileName,
            FileType = fileType,
            Folder = parent!
        };

        _context.Files.Add(fileEntry);
        await _context.SaveChangesAsync();

        return fileEntry.FileEntryId;
    }
}