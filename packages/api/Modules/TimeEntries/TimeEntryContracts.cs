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
