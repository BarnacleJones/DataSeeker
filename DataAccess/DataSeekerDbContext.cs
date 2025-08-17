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
    public DbSet<Folder> Folders { get; set; }
    public DbSet<FileEntry> Files { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Folder>()
            .HasOne(f => f.Parent)
            .WithMany(f => f.SubFolders)
            .HasForeignKey(f => f.ParentId);
    }
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
