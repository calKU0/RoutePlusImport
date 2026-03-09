using RoutePlusImport.Contracts.Models;
using RoutePlusImport.Contracts.Repositories;
using RoutePlusImport.Infrastructure.Data;
using System.Data;

namespace RoutePlusImport.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly IDbExecutor _context;
        public ClientRepository(IDbExecutor context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ClientAddress>> GetClientAddresses()
        {
            return await _context.QueryAsync<ClientAddress>(
                "[dbo].[RouteGetClientsToVisit]",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<PlannedVisitDate>> GetClientPlannedDates()
        {
            return await _context.QueryAsync<PlannedVisitDate>(
                "[dbo].[RouteGetPlannedVisitDates]",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ClientVisit>> GetClientVisits(int daysBack)
        {
            return await _context.QueryAsync<ClientVisit>(
                "[dbo].[RouteGetVisits]",
                new { DaysBack = daysBack },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> InsertClientTask(ClientTask task)
        {
            var rows = await _context.QuerySingleOrDefaultAsync<int>(
                "[dbo].[RouteInsertTask]",
                task,
                commandType: CommandType.StoredProcedure);

            return rows > 0;
        }

        public async Task<bool> UpdateClientPlannedDates(PlannedVisitDate plannedVisit)
        {
            var rows = await _context.QuerySingleOrDefaultAsync<int>(
                "[dbo].[RouteUpdateClientPlannedVisitDate]",
                plannedVisit,
                commandType: CommandType.StoredProcedure);

            return rows > 0;
        }
    }
}
