using RoutePlusImport.Contracts.Models;

namespace RoutePlusImport.Contracts.Repositories
{
    public interface IClientRepository
    {
        Task<IEnumerable<ClientAddress>> GetClientAddresses();
        Task<IEnumerable<ClientVisit>> GetClientVisits(int daysBack);
        Task<IEnumerable<PlannedVisitDate>> GetClientPlannedDates();
        Task<bool> UpdateClientPlannedDates(PlannedVisitDate plannedVisit);
        Task<bool> InsertClientTask(ClientTask task);
    }
}
