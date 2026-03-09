namespace RoutePlusImport.Contracts.Services
{
    public interface IClientDataService
    {
        Task ProcessClientVisitsAsync();
        Task ProcessClientAddressesAsync();
        Task ProcessRoutePointsAsync();
    }
}
