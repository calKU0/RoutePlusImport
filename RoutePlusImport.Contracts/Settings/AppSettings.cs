namespace RoutePlusImport.Contracts.Settings
{
    public class AppSettings
    {
        public int SendingHour { get; set; }
        public int DownloadHour { get; set; }
        public int DownloadDay { get; set; }
        public int BackVisitsDays { get; set; }
        public int WorkingIntervalMinutes { get; set; }
        public int LogsExpirationDays { get; set; }
        public string ExportDirectory { get; set; }
    }
}
