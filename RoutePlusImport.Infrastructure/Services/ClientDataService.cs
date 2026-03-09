using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoutePlusImport.Contracts.DTOs;
using RoutePlusImport.Contracts.Models;
using RoutePlusImport.Contracts.Repositories;
using RoutePlusImport.Contracts.Services;
using RoutePlusImport.Contracts.Settings;

namespace RoutePlusImport.Infrastructure.Services
{
    public class ClientDataService : IClientDataService
    {
        private readonly IClientRepository _clientRepository;
        private readonly ICsvExportService _csvExportService;
        private readonly ICsvImportService _csvImportService;
        private readonly IFtpService _ftpService;
        private readonly ILogger<ClientDataService> _logger;
        private readonly AppSettings _appSettings;

        public ClientDataService(
            IClientRepository clientRepository,
            ICsvExportService csvExportService,
            ICsvImportService csvImportService,
            IFtpService ftpService,
            ILogger<ClientDataService> logger,
            IOptions<AppSettings> appSettings)
        {
            _clientRepository = clientRepository;
            _csvExportService = csvExportService;
            _csvImportService = csvImportService;
            _ftpService = ftpService;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task ProcessClientVisitsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to process client visits...");

                var visits = await _clientRepository.GetClientVisits(_appSettings.BackVisitsDays);
                var fileName = $"import_wizyta_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var filePath = await _csvExportService.ExportToCsvAsync(visits, fileName);

                _logger.LogInformation("Client visits exported to {FilePath}", filePath);

                await _ftpService.UploadFileAsync(filePath, fileName);

                _logger.LogInformation("Client visits uploaded to FTP: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing client visits");
            }
        }

        public async Task ProcessClientAddressesAsync()
        {
            try
            {
                _logger.LogInformation("Starting to process client addresses...");

                var addresses = await _clientRepository.GetClientAddresses();
                var fileName = $"import_klient_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var filePath = await _csvExportService.ExportToCsvAsync(addresses, fileName);

                _logger.LogInformation("Client addresses exported to {FilePath}", filePath);

                await _ftpService.UploadFileAsync(filePath, fileName);

                _logger.LogInformation("Client addresses uploaded to FTP: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing client addresses");
            }
        }

        public async Task ProcessRoutePointsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to process route points from FTP...");

                var downloadDirectory = Path.Combine(AppContext.BaseDirectory, _appSettings.ExportDirectory, "Downloads");
                var downloadedFile = await _ftpService.DownloadTodayFileAsync(downloadDirectory);

                if (downloadedFile == null)
                {
                    _logger.LogWarning("No files modified today found on FTP to download");
                    return;
                }

                _logger.LogInformation("Downloaded file: {FilePath}", downloadedFile);

                var routePoints = await _csvImportService.ImportFromCsvAsync<RoutePoint>(downloadedFile);
                var routePointsList = routePoints.ToList();

                _logger.LogInformation("Imported {Count} route points from CSV", routePointsList.Count);

                var plannedDates = await _clientRepository.GetClientPlannedDates();
                var plannedDatesDict = plannedDates.ToDictionary(pd => pd.ClientId);

                var twoWeeksFromNow = DateTime.Today.AddDays(14);
                var today = DateTime.Today;

                await UpdatePlannedDatesFromVisits(routePointsList, plannedDatesDict);

                var validRoutePoints = routePointsList.Where(rp =>
                {
                    if (DateTime.TryParseExact(
                        rp.Date,
                        "yyyyMMdd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var visitDate))
                    {
                        return visitDate >= today && visitDate <= twoWeeksFromNow;
                    }
                    return false;
                }).ToList();

                _logger.LogInformation("Filtered to {Count} route points within 2 weeks", validRoutePoints.Count);

                int successCount = 0;
                int failCount = 0;

                foreach (var routePoint in validRoutePoints)
                {
                    try
                    {
                        var task = MapRoutePointToClientTask(routePoint);
                        var success = await _clientRepository.InsertClientTask(task);

                        if (success)
                            successCount++;
                        else
                            failCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error upserting task for client {ClientId}", routePoint.ClientId);
                        failCount++;
                    }
                }

                _logger.LogInformation("Route points processing completed. Success: {Success}, Failed: {Failed}",
                    successCount, failCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing route points");
            }
        }

        private ClientTask MapRoutePointToClientTask(RoutePoint routePoint)
        {
            if (!DateTime.TryParseExact(
                routePoint.Date,
                "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var visitDate))
            {
                // Fallback to standard parsing if format doesn't match
                DateTime.TryParse(routePoint.Date, out visitDate);
            }

            int.TryParse(routePoint.VisitMinutes, out var durationMinutes);

            TimeSpan startTime = routePoint.Lp switch
            {
                1 => new TimeSpan(8, 0, 0),
                2 => new TimeSpan(9, 0, 0),
                3 => new TimeSpan(10, 0, 0),
                4 => new TimeSpan(11, 0, 0),
                5 => new TimeSpan(12, 0, 0),
                6 => new TimeSpan(13, 0, 0),
                7 => new TimeSpan(14, 0, 0),
                8 => new TimeSpan(15, 0, 0),
                9 => new TimeSpan(16, 0, 0),
                10 => new TimeSpan(17, 0, 0),
                _ => TimeSpan.TryParse(routePoint.StartVisitTime, out var originalTime)
                    ? originalTime
                    : new TimeSpan(8, 0, 0)
            };

            var startDateTime = visitDate.Add(startTime);
            var endDateTime = startDateTime.AddMinutes(durationMinutes);

            return new ClientTask
            {
                ContractorId = routePoint.ClientId,
                AssigneeId = routePoint.ManagerId,
                StartDate = startDateTime,
                EndDate = endDateTime,
                CreatedDate = DateTime.Now,
                Info = $"Wizyta Route: {routePoint.ClientName}",
                OperatorId = routePoint.ManagerId
            };
        }

        private async Task UpdatePlannedDatesFromVisits(List<RoutePoint> routePoints, Dictionary<int, PlannedVisitDate> plannedDatesDict)
        {
            try
            {
                _logger.LogInformation("Starting to update planned visit dates from file...");

                int updatedCount = 0;
                int skippedCount = 0;

                // Group route points by client
                var clientGroups = routePoints.GroupBy(rp => rp.ClientId);

                foreach (var clientGroup in clientGroups)
                {
                    var clientId = clientGroup.Key;

                    if (!plannedDatesDict.TryGetValue(clientId, out var plannedDate))
                    {
                        skippedCount++;
                        continue;
                    }

                    var visitDatesFromFile = clientGroup
                        .Select(rp =>
                        {
                            if (DateTime.TryParseExact(
                                rp.Date,
                                "yyyyMMdd",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out var visitDate))
                            {
                                return (DateTime?)visitDate;
                            }
                            return null;
                        })
                        .Where(d => d.HasValue)
                        .Select(d => d.Value)
                        .OrderBy(d => d)
                        .ToList();

                    if (!visitDatesFromFile.Any())
                    {
                        skippedCount++;
                        continue;
                    }

                    var plannedDates = new List<(int Index, DateTime? Date)>
                    {
                        (1, plannedDate.Date1),
                        (2, plannedDate.Date2),
                        (3, plannedDate.Date3),
                        (4, plannedDate.Date4),
                        (5, plannedDate.Date5),
                        (6, plannedDate.Date6)
                    };

                    var mappedDates = MapVisitsToPlannedDates(plannedDates, visitDatesFromFile);

                    DateTime? GetUpdatedDate(int index, DateTime? originalDate)
                    {
                        // If original date is in the past, keep it as is
                        if (originalDate.HasValue && originalDate.Value < DateTime.Today)
                            return originalDate;

                        // If we have a mapped visit, use it; otherwise null
                        return mappedDates.ContainsKey(index) ? mappedDates[index] : null;
                    }

                    var updatedPlannedDate = new PlannedVisitDate
                    {
                        ClientId = clientId,
                        Date1 = GetUpdatedDate(1, plannedDate.Date1),
                        Date2 = GetUpdatedDate(2, plannedDate.Date2),
                        Date3 = GetUpdatedDate(3, plannedDate.Date3),
                        Date4 = GetUpdatedDate(4, plannedDate.Date4),
                        Date5 = GetUpdatedDate(5, plannedDate.Date5),
                        Date6 = GetUpdatedDate(6, plannedDate.Date6)
                    };

                    var success = await _clientRepository.UpdateClientPlannedDates(updatedPlannedDate);

                    if (success)
                    {
                        updatedCount++;
                        _logger.LogInformation(
                            "Updated planned dates for client {ClientId}: D1={Date1}, D2={Date2}, D3={Date3}, D4={Date4}, D5={Date5}, D6={Date6}",
                            clientId,
                            updatedPlannedDate.Date1?.ToString("yyyy-MM-dd") ?? "null",
                            updatedPlannedDate.Date2?.ToString("yyyy-MM-dd") ?? "null",
                            updatedPlannedDate.Date3?.ToString("yyyy-MM-dd") ?? "null",
                            updatedPlannedDate.Date4?.ToString("yyyy-MM-dd") ?? "null",
                            updatedPlannedDate.Date5?.ToString("yyyy-MM-dd") ?? "null",
                            updatedPlannedDate.Date6?.ToString("yyyy-MM-dd") ?? "null");
                    }
                }

                _logger.LogInformation(
                    "Planned visit dates update completed. Updated: {Updated}, Skipped: {Skipped}",
                    updatedCount,
                    skippedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating planned visit dates from file");
            }
        }

        private Dictionary<int, DateTime?> MapVisitsToPlannedDates(List<(int Index, DateTime? Date)> plannedDates, List<DateTime> visitDatesFromFile)
        {
            var result = new Dictionary<int, DateTime?>();
            var usedVisits = new HashSet<DateTime>();

            var validPlannedDates = plannedDates
                .Where(pd => pd.Date.HasValue && pd.Date.Value >= DateTime.Today)
                .OrderBy(pd => pd.Date.Value)
                .ToList();

            foreach (var visit in visitDatesFromFile.OrderBy(v => v))
            {
                var nearestPlanned = validPlannedDates
                    .Where(pd => !result.ContainsKey(pd.Index))
                    .OrderBy(pd => Math.Abs((pd.Date.Value - visit).TotalDays))
                    .FirstOrDefault();

                if (nearestPlanned.Index > 0)
                {
                    result[nearestPlanned.Index] = visit;
                }
            }

            return result;
        }
    }
}
