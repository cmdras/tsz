namespace Api.Modules.TimeEntries;

public record WeekChipResponse(string Label, decimal Hours);

public record WeekPreviousSummaryResponse(IReadOnlyList<WeekChipResponse> Chips, string? Overflow);

public record WeekResponse(
    DateOnly WeekStart,
    bool IsSubmitted,
    DateTime? SubmittedAt,
    DateTime? LastSavedAt,
    IReadOnlyList<object> Rows,
    WeekPreviousSummaryResponse PreviousWeekSummary);

public record PickerTaskOption(Guid ContractTaskId, string CustomerName, string ContractSubject, string TaskName);

public record PickerLeaveTypeOption(Guid LeaveTypeId, string Name);

public record PickerOptions(
    IReadOnlyList<PickerTaskOption> AvailableTasks,
    IReadOnlyList<PickerLeaveTypeOption> AvailableLeaveTypes);
