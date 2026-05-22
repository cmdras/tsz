using Api.Modules.Contracts;
using Api.Modules.LeaveTypes;

namespace Api.Modules.TimeEntries;

public interface ITimeEntryRepository
{
    Task<WeekData> GetWeekAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default);
    Task<PickerRawData> GetPickerDataAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeEntry>> GetWeekEntriesAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default);
    Task ApplyWeekDiffAsync(Guid userId, IReadOnlyList<WeekCell> toUpsert, IReadOnlyList<Guid> toDeleteIds, DateTime updatedAt, CancellationToken cancellationToken = default);
}

public record WeekData(bool IsSubmitted, DateTime? SubmittedAt, DateTime? LastSavedAt);

public record PickerRawData(
    IReadOnlyList<Contract> Contracts,
    IReadOnlyList<LeaveType> LeaveTypes,
    IReadOnlyList<Guid> AlreadyOnGrid);
