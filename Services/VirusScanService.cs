using nClam;
using SmartFileUploadService.Services.Interfaces;
using SmartFileUploadService.Models;

namespace SmartFileUploadService.Services
{
    public class VirusScanService : IVirusScanner
    {
        private readonly IConfiguration _config;
        private readonly ILogger<VirusScanService> _logger;

        public VirusScanService(IConfiguration config, ILogger<VirusScanService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<ScanResult> ScanFileAsync(Stream fileStream, string fileName)
        {
            var result = new ScanResult();

            try
            {
                var clamAvServer = _config["ClamAVSettings:Server"];
                var clamAvPort = _config.GetValue<int>("ClamAVSettings:Port", 3310);

                // Убираем using, так как ClamClient не реализует IDisposable
                var clam = new ClamClient(clamAvServer, clamAvPort);

                // Сбросим позицию потока на случай, если она была изменена
                if (fileStream.CanSeek)
                    fileStream.Position = 0;

                var scanResult = await clam.SendAndScanFileAsync(fileStream);

                switch (scanResult.Result)
                {
                    case ClamScanResults.Clean:
                        result.IsSuccess = true;
                        result.IsSecure = true;
                        result.Message = "File is clean!";
                        _logger.LogInformation("File {FileName} is clean.", fileName);
                        break;
                    case ClamScanResults.VirusDetected:
                        result.IsSuccess = true;
                        result.IsSecure = false;
                        result.Message = $"Virus detected: {string.Join(", ", scanResult.InfectedFiles?.Select(f => f.VirusName) ?? new List<string>())}";
                        _logger.LogWarning("Virus detected in file {FileName}: {VirusName}", fileName, result.Message);
                        break;
                    case ClamScanResults.Error:
                        result.IsSuccess = false;
                        result.IsSecure = false;
                        result.Message = $"Error scanning file: {scanResult.RawResult}";
                        _logger.LogError("Error scanning file {FileName}: {Error}", fileName, scanResult.RawResult);
                        break;
                    default:
                        result.IsSuccess = false;
                        result.IsSecure = false;
                        result.Message = "Unknown scan result.";
                        break;
                }
                result.RawResult = scanResult.RawResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Virus scan failed for file {FileName}.", fileName);
                result.IsSuccess = false;
                result.IsSecure = false;
                result.Message = $"Scan failed: {ex.Message}";
            }

            return result;
        }
    }
}