using Api.Common.Database;
using Api.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.LeaveOverview;

public class InvalidLeaveOverviewRequestException(string message) : DomainException(message, 400);

public class LeaveOverviewService(AppDbContext dbContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<LeaveOverviewResponse> GetOverviewAsync(
        Guid userId,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (year < 1 || year > 9999)
            throw new InvalidLeaveOverviewRequestException($"year must be between 1 and 9999. Got: {year}.");

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
