# RoutePlus Import Service

> 💼 **Commercial Project** — part of a client-facing initiative.

A .NET 10 Worker Service application that automates bidirectional data synchronization between a route optimization system (RoutePlus) and the GoNet CRM system via SFTP.

## Overview

RoutePlus Import Service is an enterprise-grade background service designed to streamline field service operations by:
- Exporting client visit schedules and address data to RoutePlus for route optimization
- Importing optimized routes back into the GoNet system
- Automatically updating planned visit dates based on actual route assignments
- Creating scheduled tasks in GoNet based on route optimization results

## Features

### 📤 **Data Export (GoNet → RoutePlus)**
- **Client Visits Export**: Retrieves recent client visits from GoNet database
- **Client Addresses Export**: Exports client address information with visit priorities
- **CSV Generation**: Converts data to CSV format with customizable column mappings
- **SFTP Upload**: Securely uploads CSV files to RoutePlus SFTP server

### 📥 **Data Import (RoutePlus → GoNet)**
- **Route Points Download**: Downloads optimized route files from SFTP (only today's files)
- **CSV Parsing**: Imports route points with visit dates, times, and sequences
- **Task Creation**: Automatically creates tasks in GoNet for each optimized visit
- **Date Filtering**: Only processes routes within a 2-week timeframe

### 🔄 **Intelligent Date Synchronization**
- **Planned Date Matching**: Maps actual visits from route optimization to planned visit dates
- **1:1 Visit Assignment**: Ensures each optimized visit is assigned to the nearest planned date
- **Past Date Protection**: Preserves historical planned dates
- **Null Handling**: Sets unmatched planned dates to null for clarity

### ⏰ **Smart Scheduling**
- **Time-Based Execution**: Runs export at sending hour, import at download hour
- **Once-Daily Processing**: Prevents duplicate processing with date tracking
- **Sequence-Based Timing**: Automatically assigns hourly time slots (8 AM - 5 PM) based on visit sequence

## Architecture

### Project Structure

```
RoutePlusImport/
├── RoutePlusImport.Service/          # Worker Service (entry point)
│   ├── Worker.cs                     # Background service orchestrator
│   ├── Program.cs                    # Service configuration & DI
│   └── appsettings.json              # Configuration
├── RoutePlusImport.Contracts/        # Interfaces & DTOs
│   ├── Models/                       # Domain models
│   │   ├── ClientAddress.cs
│   │   ├── ClientVisit.cs
│   │   ├── ClientTask.cs
│   │   └── PlannedVisitDate.cs
│   ├── DTOs/                         # Data transfer objects
│   │   └── RoutePoint.cs
│   ├── Services/                     # Service interfaces
│   │   ├── IClientDataService.cs
│   │   ├── ICsvExportService.cs
│   │   ├── ICsvImportService.cs
│   │   └── IFtpService.cs
│   ├── Repositories/                 # Repository interfaces
│   │   └── IClientRepository.cs
│   ├── Attributes/                   # Custom attributes
│   │   └── CsvColumnAttribute.cs
│   └── Settings/                     # Configuration models
│       ├── AppSettings.cs
│       └── FtpSettings.cs
└── RoutePlusImport.Infrastructure/   # Implementations
    ├── Data/                         # Database access
    │   ├── DapperDbExecutor.cs
    │   └── IDbExecutor.cs
    ├── Repositories/                 # Repository implementations
    │   └── ClientRepository.cs
    └── Services/                     # Business logic services
        ├── ClientDataService.cs
        ├── CsvExportService.cs
        ├── CsvImportService.cs
        └── FtpService.cs
```

### Key Components

#### **Worker Service**
- Runs continuously as a Windows Service
- Executes tasks based on configured schedules
- Manages execution state to prevent duplicate runs
- Logs all operations for monitoring and troubleshooting

#### **ClientDataService**
Core orchestration service handling:
- Client visits and addresses processing
- Route points import and task creation
- Planned visit date synchronization with intelligent matching algorithm

#### **CSV Services**
- **CsvExportService**: Generates CSV files with attribute-based column mapping using `[CsvColumn]` attributes
- **CsvImportService**: Parses CSV files with support for:
  - Quoted fields and escaped characters
  - Type conversion (string, int, DateTime, TimeSpan, etc.)
  - Custom column name mapping via attributes

#### **FTP Service**
- SFTP client using SSH.NET for secure file transfer
- Upload/download with file modification date filtering
- Support for both input and output folder paths
- Connection management with proper dispose patterns

#### **Repository Layer**
- **ClientRepository**: Database operations via stored procedures
- Uses Dapper for high-performance data access
- Supports complex multi-table queries and updates
- Handles nullable DateTime types for flexible date management

## Technologies

- **.NET 10**: Latest .NET framework with modern C# features
- **C# 14**: Modern language features (file-scoped namespaces, global usings, etc.)
- **Dapper**: Micro-ORM for high-performance database access
- **SSH.NET**: SFTP client library for secure file transfers
- **Serilog**: Structured logging with rolling file support
- **Microsoft.Extensions.Hosting**: Worker Service framework
- **Microsoft.Data.SqlClient**: SQL Server database connectivity

## Logging

The service uses structured logging with Serilog:

- **Console Output**: Real-time monitoring during development
- **Rolling File Logs**: Daily log files with configurable retention
- **Log Levels**: Information, Warning, Error
- **Contextual Logging**: Includes operation details, client IDs, file paths, counts
- **Separate Log Directory**: Configurable via `AppSettings:LogsExpirationDays`

**Example Log Output:**
```
[2026-03-06 15:00:00 INF] Worker running at: 06.03.2026 15:00:00 +01:00
[2026-03-06 15:00:01 INF] Processing route points...
[2026-03-06 15:00:02 INF] Downloaded file: C:\Temp\Exports\Downloads\route_20260306.csv
[2026-03-06 15:00:03 INF] Imported 156 route points from CSV
[2026-03-06 15:00:04 INF] Filtered to 89 route points within 2 weeks
[2026-03-06 15:00:05 INF] Updated planned dates for client 12345: D1=2026-05-26, D2=null, D3=2026-08-12
[2026-03-06 15:00:10 INF] Route points processing completed. Success: 89, Failed: 0
```

## Error Handling

- **Graceful Degradation**: Continues processing on individual failures
- **Try-Catch Blocks**: Comprehensive error handling at all levels
- **Detailed Error Logging**: Full exception details with stack traces
- **Transaction Safety**: Database operations use stored procedures with proper error handling
- **SFTP Resilience**: Proper connection disposal and error recovery
- **CSV Parsing**: Handles malformed data with logging

## Performance

- **Async/Await**: Fully asynchronous for optimal resource utilization
- **Batch Processing**: Efficient bulk operations for large datasets
- **Connection Pooling**: Reuses database connections via Dapper
- **SFTP Streaming**: Memory-efficient file transfers
- **Date Filtering**: Processes only relevant data (2-week window)
- **Single Daily Execution**: Prevents unnecessary resource usage

## Security

- **SFTP Encryption**: All file transfers use SSH protocol (port 22)
- **Parameterized Queries**: Protection against SQL injection via Dapper
- **Credential Management**: Stored in `appsettings.json` (use secure config in production)
- **Connection Timeout**: Prevents hung connections
- **Input Validation**: Date parsing with error handling
- **Linked Server Access**: Controlled via SQL Server OPENQUERY

## Scheduling

The Worker Service executes on a configurable schedule:

**Daily Schedule:**
```
08:00 (SendingHour)     → Export client visits and addresses to RoutePlus
15:00 (DownloadHour)    → Import route points and update planned dates
```

**Execution Control:**
- Checks every `WorkingIntervalMinutes` (configurable)
- Tracks last execution date to prevent duplicate runs
- Only processes files modified today from SFTP

## License

This project is **proprietary and confidential**.

It was developed for a client and is **not permitted to be shared, redistributed, or used** without explicit written permission from the owner.

See [LICENSE](LICENSE) for details.

---

© 2026-present [calKU0](https://github.com/calKU0)