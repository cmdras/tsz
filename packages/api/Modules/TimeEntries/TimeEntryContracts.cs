namespace Api.Modules.TimeEntries;

public record WeekChipResponse(string Label, decimal Hours);

public record WeekPreviousSummaryResponse(IReadOnlyList<WeekChipResponse> Chips, string? Overflow);

public record WeekRowResponse(
    Guid? ContractTaskId,
    string? CustomerName,
    string? ContractSubject,
    string? TaskName,
    Guid? LeaveTypeId,
    string? LeaveTypeName,
    IReadOnlyList<decimal?> Hours);

public record WeekResponse(
    DateOnly WeekStart,
    bool IsSubmitted,
    DateTime? SubmittedAt,
    DateTime? LastSavedAt,
    IReadOnlyList<WeekRowResponse> Rows,
    WeekPreviousSummaryResponse PreviousWeekSummary);

public record WeekCell(
    Guid? ContractTaskId,
    Guid? LeaveTypeId,
    DateOnly Date,
    decimal Hours);

public record UpdateWeekRequest(IReadOnlyList<WeekCell> Cells);

public record PickerTaskOption(Guid ContractTaskId, string CustomerName, string ContractSubject, string TaskName);

public record PickerLeaveTypeOption(Guid LeaveTypeId, string Name);

public record PickerOptions(
    IReadOnlyList<PickerTaskOption> AvailableTasks,
    IReadOnlyList<PickerLeaveTypeOption> AvailableLeaveTypes);

public record MonthEntryResponse(Guid Id, decimal Hours, Guid? ContractTaskId, Guid? LeaveTypeId);

public record MonthDayResponse(
    DateOnly Date,
    bool IsInMonth,
    decimal TotalHours,
    IReadOnlyList<MonthEntryResponse> Entries);

public record WeekSubmissionStatusResponse(DateOnly WeekStart, bool IsSubmitted);

public record MonthResponse(
    string YearMonth,
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyList<MonthDayResponse> Days,
    IReadOnlyList<WeekSubmissionStatusResponse> WeekSubmissions);
