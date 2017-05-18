using System;

namespace FileStore.Models.ResponseResults
{
  public class FileMetadataResult
  {
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public long Size { get; set; }
  }
}
