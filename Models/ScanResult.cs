namespace SmartFileUploadService.Models
{
    public class ScanResult
    {
        public bool IsSuccess { get; set; }
        public bool IsSecure { get; set; }
        public string? Message { get; set; }
        public string? RawResult { get; set; }
    }
}