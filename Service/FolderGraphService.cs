using Microsoft.EntityFrameworkCore;
using Service.Contract;
using Service.Models;

namespace Service
{
    public class FolderGraphService : IFolderGraphService
    {
        private readonly DataSeekerDbContext _context;

        public FolderGraphService(DataSeekerDbContext context)
        {
            _context = context;
        }

        public async Task<GraphDto> GetFolderGraphAsync()
        {
            var folders = await _context.LocalFolders
                .Include(f => f.SubFolders)
                .ToListAsync();

            var graph = new GraphDto();

            // Nodes
            foreach (var folder in folders)
            {
                graph.Nodes.Add(new NodeDto
                {
                    Id = folder.LocalFolderId.ToString(),
                    Label = folder.Name
                });
            }

            // Links
            foreach (var folder in folders)
            {
                if (folder.ParentFolderId != null)
                {
                    graph.Links.Add(new LinkDto
                    {
                        Source = folder.ParentFolderId.Value.ToString(),
                        Target = folder.LocalFolderId.ToString(),
                        Value = 1
                    });
                }
            }

            return graph;
        }
    }
}