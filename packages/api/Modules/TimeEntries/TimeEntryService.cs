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

        var previousWeekStart = weekStart.AddDays(-7);

        var weekData = await _repository.GetWeekAsync(userId, weekStart, cancellationToken);
        var entries = await _repository.GetWeekEntriesAsync(userId, weekStart, cancellationToken);
        var previousWeekEntries = await _repository.GetWeekEntriesAsync(userId, previousWeekStart, cancellationToken);
        var rows = BuildWeekRows(entries, weekStart);
        var previousWeekSummary = PreviousWeekAggregator.Aggregate(previousWeekEntries);

        return new WeekResponse(
            WeekStart: weekStart,
            IsSubmitted: weekData.IsSubmitted,
            SubmittedAt: weekData.SubmittedAt,
            LastSavedAt: weekData.LastSavedAt,
            Rows: rows,
            PreviousWeekSummary: previousWeekSummary);
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

    public async Task<WeekResponse> SubmitWeekAsync(Guid userId, DateOnly weekStart, UpdateWeekRequest request, CancellationToken cancellationToken = default)
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
        await _repository.SubmitWeekAsync(userId, weekStart, toUpsert, toDeleteIds, _clock.UtcNow, cancellationToken);

        return await GetWeekAsync(userId, weekStart, cancellationToken);
    }

    public async Task<MonthResponse> GetMonthAsync(Guid userId, string yearMonth, CancellationToken cancellationToken = default)
    {
        if (!TryParseYearMonth(yearMonth, out var monthStart))
            throw new InvalidTimeEntryRequestException($"yearMonth must be in YYYY-MM format. Got: {yearMonth}.");

        var grid = VisibleMonthGrid.Build(monthStart);
        var monthRawData = await _repository.GetMonthDataAsync(userId, grid.FromDate, grid.ToDate, cancellationToken);
        var days = MonthAggregator.Build(grid, monthRawData.Entries);
        var weekSubmissions = MonthAggregator.BuildWeekSubmissions(grid, monthRawData.Submissions);

        return new MonthResponse(
            YearMonth: yearMonth,
            FromDate: grid.FromDate,
            ToDate: grid.ToDate,
            Days: days,
            WeekSubmissions: weekSubmissions);
    }

    private static bool TryParseYearMonth(string value, out DateOnly result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value) || value.Length != 7 || value[4] != '-')
            return false;
        if (!int.TryParse(value.AsSpan(0, 4), out var year) || !int.TryParse(value.AsSpan(5, 2), out var month))
            return false;
        if (month < 1 || month > 12)
            return false;
        result = new DateOnly(year, month, 1);
        return true;
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
