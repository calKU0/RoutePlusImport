using RoutePlusImport.Contracts.Attributes;
using RoutePlusImport.Contracts.Services;
using System.Globalization;
using System.Reflection;

namespace RoutePlusImport.Infrastructure.Services
{
    public class CsvImportService : ICsvImportService
    {
        public async Task<IEnumerable<T>> ImportFromCsvAsync<T>(string filePath) where T : class, new()
        {
            var lines = await File.ReadAllLinesAsync(filePath);

            if (lines.Length == 0)
                return Enumerable.Empty<T>();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var headerLine = lines[0];
            var headers = ParseCsvLine(headerLine);

            var result = new List<T>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var values = ParseCsvLine(lines[i]);
                var item = new T();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    var property = properties.FirstOrDefault(p =>
                    {
                        var csvColumnAttr = p.GetCustomAttribute<CsvColumnAttribute>();
                        if (csvColumnAttr != null)
                        {
                            return csvColumnAttr.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase);
                        }
                        return p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase);
                    });

                    if (property != null && property.CanWrite)
                    {
                        var csvColumnAttr = property.GetCustomAttribute<CsvColumnAttribute>();
                        if (csvColumnAttr?.Ignore == true)
                            continue;

                        var value = ConvertValue(values[j], property.PropertyType);
                        property.SetValue(item, value);
                    }
                }

                result.Add(item);
            }

            return result;
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ';' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            values.Add(current.ToString());
            return values.ToArray();
        }

        private static object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    return Activator.CreateInstance(targetType);
                return null;
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(string))
                return value;

            if (underlyingType == typeof(int))
                return int.Parse(value);

            if (underlyingType == typeof(decimal))
                return decimal.Parse(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(double))
                return double.Parse(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(DateTime))
                return DateTime.Parse(value);

            if (underlyingType == typeof(bool))
                return bool.Parse(value);

            if (underlyingType == typeof(TimeSpan))
                return TimeSpan.Parse(value);

            return Convert.ChangeType(value, underlyingType);
        }
    }
}
