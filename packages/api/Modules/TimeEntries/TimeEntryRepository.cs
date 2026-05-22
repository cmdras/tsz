using Api.Common.Database;
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

    public async Task<IReadOnlyList<TimeEntry>> GetWeekEntriesAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        var weekEnd = weekStart.AddDays(7);
        return await _dbContext.TimeEntries
            .Include(entry => entry.ContractTask)
                .ThenInclude(task => task!.Contract)
                    .ThenInclude(contract => contract.Customer)
            .Where(entry => entry.UserId == userId && entry.Date >= weekStart && entry.Date < weekEnd)
            .ToListAsync(cancellationToken);
    }

    public async Task ApplyWeekDiffAsync(Guid userId, IReadOnlyList<WeekCell> toUpsert, IReadOnlyList<Guid> toDeleteIds, DateTime updatedAt, CancellationToken cancellationToken = default)
    {
        if (toDeleteIds.Count > 0)
        {
            var toDelete = await _dbContext.TimeEntries
                .Where(entry => toDeleteIds.Contains(entry.Id))
                .ToListAsync(cancellationToken);
            _dbContext.TimeEntries.RemoveRange(toDelete);
        }

        foreach (var cell in toUpsert)
        {
            var existing = await _dbContext.TimeEntries.FirstOrDefaultAsync(
                entry => entry.UserId == userId
                    && entry.Date == cell.Date
                    && entry.ContractTaskId == cell.ContractTaskId
                    && entry.LeaveTypeId == cell.LeaveTypeId,
                cancellationToken);

            if (existing is not null)
            {
                existing.Hours = cell.Hours;
                existing.UpdatedAt = updatedAt;
            }
            else
            {
                _dbContext.TimeEntries.Add(new TimeEntry
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = cell.Date,
                    ContractTaskId = cell.ContractTaskId,
                    LeaveTypeId = cell.LeaveTypeId,
                    Hours = cell.Hours,
                    UpdatedAt = updatedAt,
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
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
