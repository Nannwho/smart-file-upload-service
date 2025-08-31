namespace SmartFileUploadService.Models
{
    public class FileUploadResult
    {
        public bool IsUploaded { get; set; }
        public bool IsScanned { get; set; }
        public string? FileName { get; set; }
        public string? StoredFileName { get; set; }
        public string? Message { get; set; }
        public long FileSize { get; set; }
        public string? ScanResult { get; set; }
        public string? DownloadUrl { get; set; } // Для чистых файлов
    }
}