using Microsoft.AspNetCore.Mvc;
using SmartFileUploadService.Services;
using SmartFileUploadService.Services.Interfaces;
using SmartFileUploadService.Models;

namespace SmartFileUploadService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IVirusScanner _virusScanner;
        private readonly FileStorageService _storageService;
        private readonly IConfiguration _config;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IVirusScanner virusScanner, FileStorageService storageService, IConfiguration config, ILogger<FileUploadController> logger)
        {
            _virusScanner = virusScanner;
            _storageService = storageService;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        [RequestSizeLimit(10_485_760)] // 10MB лимит
        public async Task<ActionResult<FileUploadResult>> UploadFile(IFormFile file)
        {
            var result = new FileUploadResult();

            // 1. Проверка на null
            if (file == null || file.Length == 0)
            {
                result.Message = "No file uploaded.";
                return BadRequest(result);
            }

            // 2. Проверка размера файла
            var maxSize = _config.GetValue<long>("FileUploadSettings:MaxFileSize", 10_485_760);
            if (file.Length > maxSize)
            {
                result.Message = $"File size exceeds the limit of {maxSize / 1024 / 1024}MB.";
                return BadRequest(result);
            }

            // 3. Проверка расширения файла
            var allowedExtensions = _config.GetSection("FileUploadSettings:AllowedExtensions").Get<string[]>();
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (allowedExtensions == null || !allowedExtensions.Contains(fileExtension))
            {
                result.Message = $"File type {fileExtension} is not allowed.";
                return BadRequest(result);
            }

            string storedFileName = null;
            try
            {
                // 4. Сохраняем файл во временную папку
                storedFileName = await _storageService.SaveFileAsync(file);
                result.FileName = file.FileName;
                result.StoredFileName = storedFileName;
                result.FileSize = file.Length;

                // 5. Сканируем файл на вирусы
                using (var stream = file.OpenReadStream())
                {
                    var scanResult = await _virusScanner.ScanFileAsync(stream, file.FileName);
                    result.IsScanned = scanResult.IsSuccess;
                    result.ScanResult = scanResult.Message;

                    if (scanResult.IsSuccess && scanResult.IsSecure)
                    {
                        // Файл чистый -> перемещаем в папку "clean"
                        _storageService.MoveFile(
                            storedFileName,
                            _storageService.GetUploadPath(),
                            _storageService.GetCleanPath()
                        );
                        result.IsUploaded = true;
                        result.Message = "File uploaded and scanned successfully.";
                        result.DownloadUrl = Url.Action("Download", new { fileName = storedFileName }); // Генерируем URL
                        _logger.LogInformation("File {FileName} uploaded successfully.", file.FileName);
                        return Ok(result);
                    }
                    else
                    {
                        // Файл заражен или ошибка сканирования -> перемещаем в карантин
                        _storageService.MoveFile(
                            storedFileName,
                            _storageService.GetUploadPath(),
                            _storageService.GetQuarantinePath()
                        );
                        result.IsUploaded = false;
                        result.Message = $"File rejected: {scanResult.Message}";
                        _logger.LogWarning("File {FileName} rejected. Reason: {Reason}", file.FileName, scanResult.Message);
                        return UnprocessableEntity(result); // HTTP 422
                    }
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки пытаемся удалить временный файл
                if (storedFileName != null)
                {
                    var filePath = Path.Combine(_storageService.GetUploadPath(), storedFileName);
                    _storageService.DeleteFile(filePath);
                }

                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                result.Message = $"An error occurred: {ex.Message}";
                return StatusCode(500, result);
            }
        }

        [HttpGet("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(_storageService.GetCleanPath(), fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Здесь можно определить MIME-тип на основе расширения файла
            var mimeType = "application/octet-stream";
            return PhysicalFile(filePath, mimeType, Path.GetFileName(filePath));
        }
    }
}