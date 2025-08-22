using DataAccess;
using DataAccess.Contract;
using Microsoft.AspNetCore.Mvc;
using Service.Contract;

namespace DataSeeker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphController : Controller
{
    private readonly IFolderGraphService _folderGraphService;

    public GraphController(IFolderGraphService  folderGraphService)
    {
        _folderGraphService = folderGraphService;
    }
    
    /// <summary>
    /// Returns folder graph (nodes + links) for visualizations like Sankey.
    /// </summary>
    [HttpGet("folders")]
    public async Task<IActionResult> GetFolderGraph()
    {
        // Example: service returns something like
        // {
        //   nodes: [{ id: "MusicBrainzMusic" }, { id: "El Michels Affair" }],
        //   links: [{ source: "MusicBrainzMusic", target: "El Michels Affair", value: 1 }]
        // }

        var graph = await _folderGraphService.GetFolderGraphAsync();

        return Ok(graph);
    }
}

