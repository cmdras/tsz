namespace Api.Modules.TimeEntries;

public interface ITimeEntryRepository
{
    Task<WeekData> GetWeekAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default);
    Task<PickerRawData> GetPickerDataAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default);
}

public record WeekData(bool IsSubmitted, DateTime? SubmittedAt, DateTime? LastSavedAt);

public record PickerRawData(
    IReadOnlyList<Api.Modules.Contracts.Contract> Contracts,
    IReadOnlyList<Api.Modules.LeaveTypes.LeaveType> LeaveTypes,
    IReadOnlyList<Guid> AlreadyOnGrid);
