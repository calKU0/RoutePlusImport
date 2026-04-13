using RoutePlusImport.Contracts.Repositories;
using RoutePlusImport.Contracts.Services;
using RoutePlusImport.Contracts.Settings;
using RoutePlusImport.Infrastructure.Data;
using RoutePlusImport.Infrastructure.Repositories;
using RoutePlusImport.Infrastructure.Services;
using RoutePlusImport.Service;
using RoutePlusImport.Service.Constants;
using RoutePlusImport.Service.Logging;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = ServiceConstants.ServiceName;
    })
    .UseSerilog((hostContext, _, loggerConfiguration) =>
    {
        loggerConfiguration.ConfigureServiceLogging(hostContext.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Configuration
        services.Configure<FtpSettings>(configuration.GetSection("FtpSettings"));
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // Database context
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddSingleton<IDbExecutor>(sp => new DapperDbExecutor(connectionString));

        // HttpClients

        // Repositories
        services.AddSingleton<IClientRepository, ClientRepository>();

        // Services
        var ftpSettings = configuration.GetSection("FtpSettings").Get<FtpSettings>()
            ?? throw new InvalidOperationException("FtpSettings not found in configuration.");
        services.AddSingleton<IFtpService>(sp => new FtpService(ftpSettings));

        services.AddSingleton<ICsvExportService, CsvExportService>();
        services.AddSingleton<ICsvImportService, CsvImportService>();
        services.AddSingleton<IClientDataService, ClientDataService>();

        // Background worker
        services.AddHostedService<Worker>();

        // Host options
        services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(15));
    })
    .Build();

host.Run();