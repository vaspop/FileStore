using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStore.Database.Models
{
  public class InitiatedFileUpload
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string Description { get; set; }

    public bool FileUploaded { get; set; }
  }
}
