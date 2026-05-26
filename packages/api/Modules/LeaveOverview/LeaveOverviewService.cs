using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.LeaveOverview;

public class LeaveOverviewService(AppDbContext dbContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<LeaveOverviewResponse> GetOverviewAsync(
        Guid userId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var leaveTypes = await _dbContext.LeaveTypes
            .Where(leaveType => !leaveType.IsArchived)
            .ToListAsync(cancellationToken);

        var userAllowances = await _dbContext.UserLeaveAllowances
            .Where(allowance => allowance.UserId == userId && allowance.Year == year)
            .ToListAsync(cancellationToken);

        var yearStart = new DateOnly(year, 1, 1);
        var yearEnd = new DateOnly(year, 12, 31);

        var timeEntries = await _dbContext.TimeEntries
            .Where(entry =>
                entry.UserId == userId &&
                entry.LeaveTypeId != null &&
                entry.Date >= yearStart &&
                entry.Date <= yearEnd)
            .ToListAsync(cancellationToken);

        return LeaveOverviewAggregator.Aggregate(year, leaveTypes, userAllowances, timeEntries);
    }
}
