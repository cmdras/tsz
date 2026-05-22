using Api.Common;
using Api.Common.Exceptions;

namespace Api.Modules.TimeEntries;

public class InvalidTimeEntryRequestException(string message) : DomainException(message, 400);
public class WeekAlreadySubmittedException() : DomainException("This week has already been submitted.", 409);

public class TimeEntryService
{
    private readonly ITimeEntryRepository _repository;
    private readonly IClock _clock;

    public TimeEntryService(ITimeEntryRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<WeekResponse> GetWeekAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        if (weekStart.DayOfWeek != DayOfWeek.Monday)
            throw new InvalidTimeEntryRequestException("weekStart must be a Monday.");

        var weekData = await _repository.GetWeekAsync(userId, weekStart, cancellationToken);
        var entries = await _repository.GetWeekEntriesAsync(userId, weekStart, cancellationToken);
        var rows = BuildWeekRows(entries, weekStart);

        return new WeekResponse(
            WeekStart: weekStart,
            IsSubmitted: weekData.IsSubmitted,
            SubmittedAt: weekData.SubmittedAt,
            LastSavedAt: weekData.LastSavedAt,
            Rows: rows,
            PreviousWeekSummary: new WeekPreviousSummaryResponse([], null));
    }

    public async Task<WeekResponse> UpdateWeekAsync(Guid userId, DateOnly weekStart, UpdateWeekRequest request, CancellationToken cancellationToken = default)
    {
        if (weekStart.DayOfWeek != DayOfWeek.Monday)
            throw new InvalidTimeEntryRequestException("weekStart must be a Monday.");

        var weekData = await _repository.GetWeekAsync(userId, weekStart, cancellationToken);
        if (weekData.IsSubmitted)
            throw new WeekAlreadySubmittedException();

        var rawData = await _repository.GetPickerDataAsync(userId, weekStart, cancellationToken);
        var validation = WeekValidator.Validate(request.Cells, weekStart, userId, rawData.Contracts, rawData.LeaveTypes);
        if (!validation.IsValid)
            throw new InvalidTimeEntryRequestException(validation.ErrorMessage!);

        var existing = await _repository.GetWeekEntriesAsync(userId, weekStart, cancellationToken);
        var (toUpsert, toDeleteIds) = WeekDiffer.Diff(existing, request.Cells);
        await _repository.ApplyWeekDiffAsync(userId, toUpsert, toDeleteIds, _clock.UtcNow, cancellationToken);

        return await GetWeekAsync(userId, weekStart, cancellationToken);
    }

    public async Task<PickerOptions> GetPickerOptionsAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        if (weekStart.DayOfWeek != DayOfWeek.Monday)
            throw new InvalidTimeEntryRequestException("weekStart must be a Monday.");

        var rawData = await _repository.GetPickerDataAsync(userId, weekStart, cancellationToken);
        return WeekScheduler.BuildPickerOptions(userId, weekStart, rawData.Contracts, rawData.LeaveTypes, rawData.AlreadyOnGridTaskIds, rawData.AlreadyOnGridLeaveTypeIds);
    }

    private static IReadOnlyList<WeekRowResponse> BuildWeekRows(IReadOnlyList<TimeEntry> entries, DateOnly weekStart)
    {
        var taskRows = entries
            .Where(entry => entry.ContractTaskId.HasValue && entry.ContractTask is not null)
            .GroupBy(entry => entry.ContractTaskId!.Value)
            .Select(group =>
            {
                var firstEntry = group.First();
                var task = firstEntry.ContractTask!;
                var contract = task.Contract;
                var hours = new decimal?[7];
                foreach (var entry in group)
                {
                    var dayIndex = entry.Date.DayNumber - weekStart.DayNumber;
                    if (dayIndex >= 0 && dayIndex < 7)
                        hours[dayIndex] = entry.Hours;
                }
                return new WeekRowResponse(
                    ContractTaskId: task.Id,
                    CustomerName: contract.Customer.Name,
                    ContractSubject: contract.Subject,
                    TaskName: task.Name,
                    LeaveTypeId: null,
                    LeaveTypeName: null,
                    Hours: hours);
            });

        var leaveRows = entries
            .Where(entry => entry.LeaveTypeId.HasValue && entry.LeaveType is not null)
            .GroupBy(entry => entry.LeaveTypeId!.Value)
            .Select(group =>
            {
                var firstEntry = group.First();
                var leaveType = firstEntry.LeaveType!;
                var hours = new decimal?[7];
                foreach (var entry in group)
                {
                    var dayIndex = entry.Date.DayNumber - weekStart.DayNumber;
                    if (dayIndex >= 0 && dayIndex < 7)
                        hours[dayIndex] = entry.Hours;
                }
                return new WeekRowResponse(
                    ContractTaskId: null,
                    CustomerName: null,
                    ContractSubject: null,
                    TaskName: null,
                    LeaveTypeId: leaveType.Id,
                    LeaveTypeName: leaveType.Name,
                    Hours: hours);
            });

        return taskRows.Concat(leaveRows).ToList();
    }
}
