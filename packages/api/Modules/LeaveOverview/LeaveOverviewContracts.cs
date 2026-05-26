namespace Api.Modules.LeaveOverview;

public record LeaveOverviewTypeItem(
    Guid Id,
    string Name,
    string Mode,
    decimal Allowance,
    decimal TakenDays);

public record LeaveOverviewDayItem(
    DateOnly Date,
    IReadOnlyList<Guid> LeaveTypeIds);

public record LeaveOverviewResponse(
    int Year,
    IReadOnlyList<LeaveOverviewTypeItem> Types,
    IReadOnlyList<LeaveOverviewDayItem> Days);
