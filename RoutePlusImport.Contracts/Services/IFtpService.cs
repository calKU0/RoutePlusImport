namespace RoutePlusImport.Contracts.Services
{
    public interface IFtpService
    {
        Task UploadFileAsync(string localFilePath, string remoteFileName);
        Task<string?> DownloadLatestFileAsync(string localDirectory, string filePattern = "*.csv");
        Task<string?> DownloadTodayFileAsync(string localDirectory, string filePattern = "*.csv");
    }
}
