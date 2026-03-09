namespace RoutePlusImport.Contracts.Services
{
    public interface ICsvExportService
    {
        Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string fileName) where T : class;
    }
}
