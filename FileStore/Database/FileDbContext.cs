using FileStore.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FileStore.Database
{
  public class FileDbContext : DbContext
  {
    public DbSet<UploadedFile> UploadedFiles { get; set; }
    public DbSet<InitiatedFileUpload> InitiatedFileUploads { get; set; }

    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
    {
    }
  }
}
