using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Stats;

public class StatsService(AppDbContext dbContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<AdminStats> GetAdminStatsAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _dbContext.Customers.CountAsync(customer => !customer.IsArchived, cancellationToken);
        var users = await _dbContext.Users.CountAsync(user => !user.IsArchived, cancellationToken);
        var contracts = await _dbContext.Contracts.CountAsync(contract => !contract.IsArchived, cancellationToken);
        var leaveTypes = await _dbContext.LeaveTypes.CountAsync(leaveType => !leaveType.IsArchived, cancellationToken);

        return new AdminStats(customers, users, contracts, leaveTypes);
    }
}
