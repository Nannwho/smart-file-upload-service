using SmartFileUploadService.Models;

namespace SmartFileUploadService.Services.Interfaces
{
    public interface IVirusScanner
    {
        Task<ScanResult> ScanFileAsync(Stream fileStream, string fileName);
    }
}