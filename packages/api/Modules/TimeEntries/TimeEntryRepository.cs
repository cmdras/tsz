using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.LeaveTypes;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.TimeEntries;

public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly AppDbContext _dbContext;

    public TimeEntryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeekData> GetWeekAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        var weekEnd = weekStart.AddDays(7);

        var submission = await _dbContext.WeekSubmissions
            .Where(submission => submission.UserId == userId && submission.WeekStart == weekStart)
            .FirstOrDefaultAsync(cancellationToken);

        var lastSavedAt = await _dbContext.TimeEntries
            .Where(entry => entry.UserId == userId && entry.Date >= weekStart && entry.Date < weekEnd)
            .MaxAsync(entry => (DateTime?)entry.UpdatedAt, cancellationToken);

        return new WeekData(submission is not null, submission?.SubmittedAt, lastSavedAt);
    }

    public async Task<PickerRawData> GetPickerDataAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        var weekEnd = weekStart.AddDays(6);

        var contracts = await _dbContext.Contracts
            .Include(contract => contract.Customer)
            .Include(contract => contract.Tasks)
            .Where(contract => contract.ConsultantId == userId)
            .ToListAsync(cancellationToken);

        var leaveTypes = await _dbContext.LeaveTypes
            .ToListAsync(cancellationToken);

        var alreadyOnGrid = await _dbContext.TimeEntries
            .Where(entry =>
                entry.UserId == userId
                && entry.Date >= weekStart
                && entry.Date <= weekEnd
                && entry.ContractTaskId != null)
            .Select(entry => entry.ContractTaskId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new PickerRawData(contracts, leaveTypes, alreadyOnGrid);
    }
}
