using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class DataSeekerDbContext(DbContextOptions<DataSeekerDbContext> options) : DbContext(options)
{
    public DbSet<UploadLine> UploadLines { get; set; }
    public DbSet<UploadFile> UploadFiles { get; set; }
    public DbSet<DownloadFile> DownloadFiles { get; set; }

}