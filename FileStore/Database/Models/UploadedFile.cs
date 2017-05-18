using System;
using System.ComponentModel.DataAnnotations;

namespace FileStore.Database.Models
{
  public class UploadedFile
  {
    [Key]
    public Guid Id { get; set; }

    public DateTime UploadedAt { get; set; }
    public long Size { get; set; }

    public string StoragePath { get; set; }

    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string Description { get; set; }
  }
}
