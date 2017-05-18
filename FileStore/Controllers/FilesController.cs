using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FileStore.Service;
using FileStore.Models.ResponseResults;
using FileStore.Models.Request;
using System.IO;

namespace FileStore.Controllers
{
  [Route("api/[controller]")]
  public class FilesController : Controller
  {
    private FileService fileService;

    /// <summary>
    /// Contructor getting the file service injected
    /// </summary>
    public FilesController(FileService fileService)
    {
      this.fileService = fileService;
    }

    /// <summary>
    /// Gets a files metadata
    /// </summary>
    /// <param name="id">File identifier</param>
    /// <returns></returns>
    [HttpGet("GetFileMetadata/{id}")]
    public async Task<IActionResult> GetFileMetadata(Guid id)
    {
      var result = await fileService.GetFileMetadata(id);

      if (!result.success)
        return BadRequest("Unable to find file info");

      return Ok(new FileMetadataResult
      {
        FileId = result.fileInfo.Id,
        ContentType = result.fileInfo.ContentType,
        Description = result.fileInfo.Description,
        FileName = result.fileInfo.FileName,
        UploadedAt = result.fileInfo.UploadedAt,
        Size = result.fileInfo.Size
      });
    }

    /// <summary>
    /// Downloads the actual file data
    /// </summary>
    /// <param name="id">File identifier</param>
    /// <returns>File stream result containing the file data</returns>
    [HttpGet("DownloadFileData/{id}")]
    public async Task<IActionResult> DownloadFileData(Guid id)
    {
      var result = await fileService.GetUploadedFile(id);

      if (!result.success)
        return BadRequest("Unable to find file");
      
      return File(result.fileStream, result.fileInfo.ContentType);
    }
    
    /// <summary>
    /// Initiates a file upload
    /// </summary>
    /// <param name="fileUploadInfo">Metadata of the file to be uploaded</param>
    /// <returns>Returns a guid which identifies the file to be uploaded. Needs to be used as the identifier for when uploading and downloading the file</returns>
    [HttpPost("InitiateFileUpload")]
    public async Task<IActionResult> InitiateFileUpload([FromBody] FileMetadata fileUploadInfo)
    {
      if(fileUploadInfo == null)
        return BadRequest();

      var savedInitiatedFileUploadInfo = await fileService.SaveInitiatedFileMetadata(fileUploadInfo);

      if(!savedInitiatedFileUploadInfo.success)
        return StatusCode(500, "Unable to initiate file upload");

      return Ok(new InitiatedFileUploadResult { GeneratedFileId = savedInitiatedFileUploadInfo.initiatedFileUpload.Id });
    }

    /// <summary>
    /// Associates/Uploads the data from the request body with the provided file id
    /// </summary>
    /// <param name="id">File identifier</param>
    /// <returns></returns>
    [HttpPost("UploadFileData/{id}")]
    public async Task<IActionResult> UploadFileData(Guid id)
    {
      var initiatedFileUpload = await fileService.GetInitiatedFileUpload(id);

      if (initiatedFileUpload == null)
        return BadRequest();

      if (initiatedFileUpload.FileUploaded)
        return BadRequest("File already uploaded");

      dynamic savedFileData;
      using (var stream = new MemoryStream())
      {
        Request.Body.CopyTo(stream);
        var fileData = stream.ToArray();
        savedFileData = await fileService.WriteFileDataToStorage(fileData, initiatedFileUpload.FileName);
      }

      if(!savedFileData.success)
        return StatusCode(500, "Unable to store file");

      var savedUploadedFileInfo = await fileService.SaveUploadedFileInfo(initiatedFileUpload, savedFileData.storagePath, savedFileData.fileSize);

      if (!savedUploadedFileInfo.success)
        return StatusCode(500, "Unable to store file");
      
      return Ok();
    }
  }
}
