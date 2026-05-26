namespace Api.Modules.TimeEntries;

public static class MonthAggregator
{
    public static IReadOnlyList<MonthDayResponse> Build(
        VisibleMonthGridResult grid,
        IReadOnlyList<TimeEntry> entries)
    {
        return grid.Days
            .Select(cell => new MonthDayResponse(
                Date: cell.Date,
                IsInMonth: cell.IsInMonth,
                TotalHours: 0m,
                Entries: []))
            .ToList();
    }
}
