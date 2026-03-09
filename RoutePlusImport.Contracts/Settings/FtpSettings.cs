namespace RoutePlusImport.Contracts.Settings
{
    public class FtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string InputFolderPath { get; set; }
        public string OutputFolderPath { get; set; }
    }
}
