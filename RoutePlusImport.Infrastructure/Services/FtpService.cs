using Renci.SshNet;
using RoutePlusImport.Contracts.Services;
using RoutePlusImport.Contracts.Settings;

namespace RoutePlusImport.Infrastructure.Services
{
    public class FtpService : IFtpService
    {
        private readonly FtpSettings _ftpSettings;

        public FtpService(FtpSettings ftpSettings)
        {
            _ftpSettings = ftpSettings;
        }

        public async Task UploadFileAsync(string localFilePath, string remoteFileName)
        {
            await Task.Run(() =>
            {
                using var client = new SftpClient(
                    _ftpSettings.Host,
                    _ftpSettings.Port,
                    _ftpSettings.Username,
                    _ftpSettings.Password);

                client.Connect();

                try
                {
                    var remoteFilePath = $"{_ftpSettings.OutputFolderPath}/{remoteFileName}".Replace("\\", "/");

                    using var fileStream = File.OpenRead(localFilePath);
                    client.UploadFile(fileStream, remoteFilePath, true);
                }
                finally
                {
                    client.Disconnect();
                }
            });
        }

        public async Task<string?> DownloadLatestFileAsync(string localDirectory, string filePattern = "*.csv")
        {
            return await Task.Run(() =>
            {
                using var client = new SftpClient(
                    _ftpSettings.Host,
                    _ftpSettings.Port,
                    _ftpSettings.Username,
                    _ftpSettings.Password);

                client.Connect();

                try
                {
                    var remoteDirectory = _ftpSettings.InputFolderPath.Replace("\\", "/");

                    var files = client.ListDirectory(remoteDirectory)
                        .Where(f => !f.IsDirectory)
                        .Where(f => filePattern == "*.*" || filePattern == "*.csv" || f.Name.EndsWith(filePattern.TrimStart('*')))
                        .OrderByDescending(f => f.LastWriteTime)
                        .ToList();

                    var latestFile = files.FirstOrDefault();

                    if (latestFile == null)
                        return null;

                    if (!Directory.Exists(localDirectory))
                    {
                        Directory.CreateDirectory(localDirectory);
                    }

                    var localFilePath = Path.Combine(localDirectory, latestFile.Name);

                    using var fileStream = File.Create(localFilePath);
                    client.DownloadFile(latestFile.FullName, fileStream);

                    return localFilePath;
                }
                finally
                {
                    client.Disconnect();
                }
            });
        }

        public async Task<string?> DownloadTodayFileAsync(string localDirectory, string filePattern = "*.csv")
        {
            return await Task.Run(() =>
            {
                using var client = new SftpClient(
                    _ftpSettings.Host,
                    _ftpSettings.Port,
                    _ftpSettings.Username,
                    _ftpSettings.Password);

                client.Connect();

                try
                {
                    var remoteDirectory = _ftpSettings.InputFolderPath.Replace("\\", "/");
                    var today = DateTime.Today;

                    var files = client.ListDirectory(remoteDirectory)
                        .Where(f => !f.IsDirectory)
                        .Where(f => filePattern == "*.*" || filePattern == "*.csv" || f.Name.EndsWith(filePattern.TrimStart('*')))
                        .Where(f => f.LastWriteTime.Date == today)
                        .OrderByDescending(f => f.LastWriteTime)
                        .ToList();

                    var latestFile = files.FirstOrDefault();

                    if (latestFile == null)
                        return null;

                    if (!Directory.Exists(localDirectory))
                    {
                        Directory.CreateDirectory(localDirectory);
                    }

                    var localFilePath = Path.Combine(localDirectory, latestFile.Name);

                    using var fileStream = File.Create(localFilePath);
                    client.DownloadFile(latestFile.FullName, fileStream);

                    return localFilePath;
                }
                finally
                {
                    client.Disconnect();
                }
            });
        }
    }
}
