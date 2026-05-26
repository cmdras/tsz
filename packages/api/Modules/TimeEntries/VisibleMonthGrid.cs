namespace Api.Modules.TimeEntries;

public record MonthDayCell(DateOnly Date, bool IsInMonth);

public record VisibleMonthGridResult(DateOnly FromDate, DateOnly ToDate, IReadOnlyList<MonthDayCell> Days);

public static class VisibleMonthGrid
{
    public static VisibleMonthGridResult Build(DateOnly yearMonth)
    {
        var firstDay = new DateOnly(yearMonth.Year, yearMonth.Month, 1);
        var lastDay = new DateOnly(yearMonth.Year, yearMonth.Month, DateTime.DaysInMonth(yearMonth.Year, yearMonth.Month));

        // Monday-anchored: days from Monday = (DayOfWeek - Monday + 7) % 7
        var daysFromMonday = ((int)firstDay.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var fromDate = firstDay.AddDays(-daysFromMonday);

        // Extend to end-of-week Sunday (DayOfWeek.Sunday == 0 in C#, so offset = (7 - dow) % 7)
        var daysToSunday = ((int)DayOfWeek.Sunday - (int)lastDay.DayOfWeek + 7) % 7;
        var toDate = lastDay.AddDays(daysToSunday);

        var days = new List<MonthDayCell>();
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            days.Add(new MonthDayCell(date, date.Month == yearMonth.Month));

        return new VisibleMonthGridResult(fromDate, toDate, days);
    }
}
