using Microsoft.Extensions.Options;
using RoutePlusImport.Contracts.Attributes;
using RoutePlusImport.Contracts.Services;
using RoutePlusImport.Contracts.Settings;
using System.Reflection;
using System.Text;

namespace RoutePlusImport.Infrastructure.Services
{
    public class CsvExportService : ICsvExportService
    {
        private readonly string _exportDirectory;

        public CsvExportService(IOptions<AppSettings> appSettings)
        {
            _exportDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appSettings.Value.ExportDirectory);
            if (!Directory.Exists(_exportDirectory))
            {
                Directory.CreateDirectory(_exportDirectory);
            }
        }

        public async Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string fileName) where T : class
        {
            var filePath = Path.Combine(_exportDirectory, fileName);
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    CsvColumn = p.GetCustomAttribute<CsvColumnAttribute>()
                })
                .Where(x => x.CsvColumn == null || !x.CsvColumn.Ignore)
                .OrderBy(x => x.CsvColumn?.Order ?? int.MaxValue)
                .ThenBy(x => x.Property.Name)
                .ToList();

            var csv = new StringBuilder();

            var headers = properties.Select(p =>
                EscapeCsvField(p.CsvColumn?.Name ?? p.Property.Name));
            csv.AppendLine(string.Join("|", headers));

            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.Property.GetValue(item);
                    return EscapeCsvField(value?.ToString() ?? string.Empty);
                });

                csv.AppendLine(string.Join("|", values));
            }

            await File.WriteAllTextAsync(filePath, csv.ToString());

            return filePath;
        }

        private static string EscapeCsvField(string field)
        {
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }
    }
}
