using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DataSeekerDbContext : DbContext
{
    public DataSeekerDbContext(DbContextOptions<DataSeekerDbContext> options)
        : base(options)
    {
    }

    public DbSet<LogFile> LogFiles { get; set; }
    public DbSet<LogLine> LogLines { get; set; }
    
    public DbSet<UploadedFile> UploadedFiles { get; set; }
    public DbSet<LocalFolder> LocalFolders { get; set; }
    
    //Mapping - todo split out
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //self-referencing one-to-many (Folder -> SubFolders). 
        modelBuilder.Entity<LocalFolder>()
            .HasOne(f => f.ParentFolder)
            .WithMany(f => f.SubFolders)
            .HasForeignKey(f => f.ParentFolderId);
        
        modelBuilder.Entity<LogFile>()
            .Property(e => e.TransferDirection)
            .HasConversion<string>(); // store enum as text instead of int
        
        //Some indexes for speedier queries
        modelBuilder.Entity<LocalFolder>()
            .HasIndex(f => f.ParentFolderId);

        modelBuilder.Entity<LocalFolder>()
            .HasIndex(f => f.Name);

        modelBuilder.Entity<LocalFolder>()
            .HasIndex(f => new { f.ParentFolderId, f.Name })
            .IsUnique();
        
        modelBuilder.Entity<UploadedFile>()
            .HasOne(f => f.ContainingFolder)
            .WithMany()
            .HasForeignKey(f => f.LocalFolderId)
            .IsRequired(false);
        
        modelBuilder.Entity<UploadedFile>()
            .HasOne(f => f.ContainingFolder)
            .WithMany(f => f.UploadedFiles)
            .HasForeignKey(f => f.LocalFolderId)
            .IsRequired(false);
        
        modelBuilder.Entity<LogFile>()
            .HasIndex(lf => lf.TransferDirection);
        
        modelBuilder.Entity<LogLine>()
            .HasIndex(ll => ll.FileType);
        
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
