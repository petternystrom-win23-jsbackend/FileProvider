using Data.Contexts;
using Data.Entities;
using FileProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProvider.Functions
{
    public class Upload(ILogger<Upload> logger, FileService fileService)
    {
        private readonly ILogger<Upload> _logger = logger;
        private readonly FileService _fileService = fileService;

        [Function("Upload")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            
            try
            {
                var formCollection = await req.ReadFormAsync();
                var containerName = !string.IsNullOrEmpty(req.Query["container"]) ? req.Query["container"].ToString() : "uploads";

                if (formCollection.Files["file"] is IFormFile file)
                {
                    var fileEntity = new FileEntity
                    {
                        FileName = _fileService.SetFileName(file),
                        ContentType = file.ContentType,
                        UploaderName = "Unknown",
                        UploadDate = DateTime.Now,
                        ContainerName = containerName
                    };
                    await _fileService.SetBlobContainerAsync(fileEntity.ContainerName);
                    var filePath = await _fileService.UploadFileAsync(file, fileEntity);
                    fileEntity.FilePath = filePath;
                    await _fileService.SaveToDataBaseAsync(fileEntity);
                    return new OkObjectResult(fileEntity);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return new BadRequestResult();

        }
    }
}
