using Data.Contexts;
using Data.Entities;
using FileProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace FileProvider.Functions
{
    public class Upload(ILogger<Upload> logger, FileService fileService)
    {
        private readonly ILogger<Upload> _logger = logger;
        private readonly FileService _fileService = fileService;

        [Function("Upload")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req, FunctionContext context)
        {
            string uploaderName = "Anonymous";
            if (req.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                var bearerToken = authHeaderValues.FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(bearerToken);
                    var usernameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "preferred_username" || claim.Type == "name");
                    uploaderName = usernameClaim?.Value ?? "Anonymous";
                }
            }
            try
            {
                var formCollection = await req.ReadFormAsync();

                if (formCollection.Files["file"] is IFormFile file)
                {
                    var fileEntity = new FileEntity
                    {
                        FileName = _fileService.SetFileName(file),
                        ContentType = file.ContentType,
                        UploaderName = uploaderName,
                        UploadDate = DateTime.Now,
                        ContainerName = "profilepictures"
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
