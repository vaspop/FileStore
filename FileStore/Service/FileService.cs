using FileStore.Database;
using FileStore.Database.Models;
using FileStore.Models;
using FileStore.Models.Request;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileStore.Service
{
  public class FileService
  {
    private FileDbContext dbContext;
    private AppSettings appSettings;

    public FileService(FileDbContext dbContext, IOptions<AppSettings> appSettings)
    {
      this.dbContext = dbContext;
      this.appSettings = appSettings.Value;
    }

    public async Task<InitiatedFileUpload> GetInitiatedFileUpload(Guid id)
    {
      return await dbContext.FindAsync<InitiatedFileUpload>(id);
    }

    public async Task<(bool success, UploadedFile uploadedFile)> SaveUploadedFileInfo(InitiatedFileUpload initiatedFileUpload, string storageFilePath, long fileSize)
    {
      try
      {
        var uploadedFile = new UploadedFile
        {
          Id = initiatedFileUpload.Id,
          FileName = initiatedFileUpload.FileName,
          ContentType = initiatedFileUpload.ContentType,
          Description = initiatedFileUpload.Description,
          StoragePath = storageFilePath,
          UploadedAt = DateTime.UtcNow,
          Size = fileSize
        };

        initiatedFileUpload.FileUploaded = true;
        dbContext.Update(initiatedFileUpload);
        await dbContext.AddAsync(uploadedFile);
        await dbContext.SaveChangesAsync();

        return (success: true, uploadedFile: uploadedFile);
      }
      catch (Exception)
      {
        return (success: false, uploadedFile: null);
      }
    }

    public async Task<(bool success, InitiatedFileUpload initiatedFileUpload)> SaveInitiatedFileMetadata(FileMetadata fileMetadata)
    {
      try
      {
        var contentType = fileMetadata.ContentType;

        if (string.IsNullOrEmpty(contentType))
          contentType = "application/octet-stream";

        var initiatedFileUpload = new InitiatedFileUpload
        {
          FileName = fileMetadata.FileName,
          ContentType = contentType,
          Description = fileMetadata.Description
        };

        await dbContext.AddAsync(initiatedFileUpload);
        await dbContext.SaveChangesAsync();

        return (success: true, initiatedFileUpload: initiatedFileUpload);
      }
      catch (Exception)
      {
        return (success: false, initiatedFileUpload: null);
      }
    }

    public async Task<(bool success, Stream fileStream, UploadedFile fileInfo)> GetUploadedFile(Guid id)
    {
      try
      {
        var uploadedFile = await dbContext.FindAsync<UploadedFile>(id);

        if (uploadedFile == null)
          return (success: false, fileStream: null, fileInfo: null);

        var fileStream = File.OpenRead(uploadedFile.StoragePath);

        return (success: true, fileStream: fileStream, fileInfo: uploadedFile);
      }
      catch (Exception)
      {
        return (success: false, fileStream: null, fileInfo: null);
      }
    }
    
    public async Task<(bool success, UploadedFile fileInfo)> GetFileMetadata(Guid id)
    {
      try
      {
        var storedFile = await dbContext.FindAsync<UploadedFile>(id);

        if (storedFile == null)
          return (success: false, fileInfo: null);
        
        return (success: true, fileInfo: storedFile);
      }
      catch (Exception)
      {
        return (success: false, fileInfo: null);
      }
    }

    public async Task<(string storagePath, long fileSize, bool success)> WriteFileDataToStorage(byte[] data, string fileName)
    {
      try
      {
        var fileStoragePath = Path.Combine(appSettings.FileStoragePath, $"{Guid.NewGuid()}_{fileName}");

        using (var file = new FileStream(fileStoragePath, FileMode.CreateNew, FileAccess.Write))
        using (var memoryStream = new MemoryStream())
        {
          await file.WriteAsync(data, 0, data.Length);
        }

        return (storagePath: fileStoragePath, fileSize: data.Length, success: true);
      }
      catch (Exception)
      {
        return (storagePath: null, fileSize: 0, success: false);
      }
    }
  }
}
