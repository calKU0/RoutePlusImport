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
        private DateTime? _lastClientProcessDate;
        private DateTime? _lastRoutePointProcessDate;

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
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    var currentDate = DateTime.Today;
                    var currentHour = DateTime.Now.Hour;

                    if (currentHour == _appSettings.SendingHour &&
                        (!_lastClientProcessDate.HasValue || _lastClientProcessDate.Value.Date < currentDate))
                    {
                        await _clientDataService.ProcessClientVisitsAsync();
                        await _clientDataService.ProcessClientAddressesAsync();
                        _lastClientProcessDate = DateTime.Now;
                    }

                    if (currentHour == _appSettings.DownloadHour &&
                        (!_lastRoutePointProcessDate.HasValue || _lastRoutePointProcessDate.Value.Date < currentDate))
                    {
                        await _clientDataService.ProcessRoutePointsAsync();
                        _lastRoutePointProcessDate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing client data");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkingIntervalMinutes), stoppingToken);
                }
            }
        }
    }
}
