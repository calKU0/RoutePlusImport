namespace RoutePlusImport.Contracts.Services
{
    public interface ICsvImportService
    {
        Task<IEnumerable<T>> ImportFromCsvAsync<T>(string filePath) where T : class, new();
    }
}
