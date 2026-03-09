using RoutePlusImport.Contracts.Repositories;
using RoutePlusImport.Contracts.Services;
using RoutePlusImport.Contracts.Settings;
using RoutePlusImport.Infrastructure.Data;
using RoutePlusImport.Infrastructure.Repositories;
using RoutePlusImport.Infrastructure.Services;
using RoutePlusImport.Service;
using RoutePlusImport.Service.Constants;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = ServiceConstants.ServiceName;
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var logsExpirationDays = Convert.ToInt32(configuration["AppSettings:LogsExpirationDays"]);
        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(logDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: logsExpirationDays,
                shared: true,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .CreateLogger();

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
    .UseSerilog()
    .Build();

host.Run();