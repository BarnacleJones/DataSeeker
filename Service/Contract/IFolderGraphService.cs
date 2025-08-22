using Service.Models;

namespace Service.Contract
{
    public interface IFolderGraphService
    {
        /// <summary>
        /// Builds a folder graph (nodes + links) suitable for visualizations.
        /// </summary>
        Task<GraphDto> GetFolderGraphAsync();
    }
}