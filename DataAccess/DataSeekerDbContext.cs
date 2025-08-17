using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DataSeekerDbContext : DbContext
{
    public DataSeekerDbContext(DbContextOptions<DataSeekerDbContext> options)
        : base(options)
    {
    }

    public DbSet<UploadLine> UploadLines { get; set; }
    public DbSet<UploadFile> UploadFiles { get; set; }
    public DbSet<DownloadFile> DownloadFiles { get; set; }
}

namespace DataAccess
{
    public class DataSeekerDbContextFactory : IDesignTimeDbContextFactory<DataSeekerDbContext>
    {
        public DataSeekerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataSeekerDbContext>();

            optionsBuilder.UseNpgsql("Host=db;Port=5432;Database=DataSeekerDb;Username=postgres;Password=radio123");

            return new DataSeekerDbContext(optionsBuilder.Options);
        }
    }
}
