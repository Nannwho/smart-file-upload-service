using SmartFileUploadService.Models;

namespace SmartFileUploadService.Services
{
    public class FileStorageService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IConfiguration config, ILogger<FileStorageService> logger)
        {
            _config = config;
            _logger = logger;
            // Создаем директории, если они не существуют
            Directory.CreateDirectory(GetUploadPath());
            Directory.CreateDirectory(GetCleanPath());
            Directory.CreateDirectory(GetQuarantinePath());
        }

        public string GetUploadPath() => Path.Combine(Directory.GetCurrentDirectory(), _config["FileUploadSettings:UploadPath"]!);
        public string GetCleanPath() => Path.Combine(Directory.GetCurrentDirectory(), _config["FileUploadSettings:CleanPath"]!);
        public string GetQuarantinePath() => Path.Combine(Directory.GetCurrentDirectory(), _config["FileUploadSettings:QuarantinePath"]!);

        public async Task<string> SaveFileAsync(IFormFile file, string? customFileName = null)
        {
            var fileName = customFileName ?? GenerateUniqueFileName(file.FileName);
            var filePath = Path.Combine(GetUploadPath(), fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File saved temporarily: {FilePath}", filePath);
            return fileName;
        }

        public void MoveFile(string fileName, string sourceDirectory, string targetDirectory)
        {
            var sourcePath = Path.Combine(sourceDirectory, fileName);
            var targetPath = Path.Combine(targetDirectory, fileName);

            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, targetPath);
                _logger.LogInformation("File moved from {Source} to {Target}", sourcePath, targetPath);
            }
        }

        public bool DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return true;
            }
            return false;
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            return $"{Guid.NewGuid()}_{Path.GetFileName(originalFileName)}";
        }
    }
}