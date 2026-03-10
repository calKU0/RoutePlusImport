using Microsoft.Extensions.Options;
using RoutePlusImport.Contracts.Services;
using RoutePlusImport.Contracts.Settings;

namespace RoutePlusImport.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AppSettings _appSettings;
        private readonly IClientDataService _clientDataService;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> appSettings, IClientDataService clientDataService)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _clientDataService = clientDataService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = GetNextScheduledRun(now);

                    var delay = nextRun - now;
                    _logger.LogInformation("Next scheduled run at: {nextRun}. Waiting for {delay}", nextRun, delay);

                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await ExecuteScheduledTask(nextRun.Hour);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing client data");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private DateTime GetNextScheduledRun(DateTime now)
        {
            var today = now.Date;
            var sendingTime = today.AddHours(_appSettings.SendingHour);
            var downloadTime = today.AddHours(_appSettings.DownloadHour);

            var candidates = new List<DateTime>();

            if (sendingTime > now)
                candidates.Add(sendingTime);
            else
                candidates.Add(sendingTime.AddDays(1));

            if (downloadTime > now)
                candidates.Add(downloadTime);
            else
                candidates.Add(downloadTime.AddDays(1));

            return candidates.Min();
        }

        private async Task ExecuteScheduledTask(int hour)
        {
            _logger.LogInformation("Executing scheduled task at hour: {hour}", hour);

            if (hour == _appSettings.SendingHour)
            {
                _logger.LogInformation("Processing client visits and addresses");
                await _clientDataService.ProcessClientVisitsAsync();
                await _clientDataService.ProcessClientAddressesAsync();
            }

            if (hour == _appSettings.DownloadHour)
            {
                var currentDay = (int)DateTime.Now.DayOfWeek;
                if (currentDay == _appSettings.DownloadDay)
                {
                    _logger.LogInformation("Processing route points on day: {day}", (DayOfWeek)currentDay);
                    await _clientDataService.ProcessRoutePointsAsync();
                }
                else
                {
                    _logger.LogInformation("Skipping route points processing. Current day: {currentDay}, Required day: {requiredDay}",
                        (DayOfWeek)currentDay, (DayOfWeek)_appSettings.DownloadDay);
                }
            }
        }
    }
}
