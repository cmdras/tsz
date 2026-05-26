using Api.Modules.TimeEntries;

namespace Api.Tests.TimeEntries;

public class VisibleMonthGridShould
{
    // --- June 2026: starts on Monday → 5-row grid ---

    [Fact]
    public void Return_Correct_Window_When_Month_Starts_On_Monday()
    {
        var june2026 = new DateOnly(2026, 6, 1);

        var grid = VisibleMonthGrid.Build(june2026);

        Assert.Equal(new DateOnly(2026, 6, 1), grid.FromDate);
        Assert.Equal(new DateOnly(2026, 7, 5), grid.ToDate);
    }

    // --- May 2026: ends on Sunday → no spill into next week ---

    [Fact]
    public void Return_Correct_Window_When_Month_Ends_On_Sunday()
    {
        var may2026 = new DateOnly(2026, 5, 1);

        var grid = VisibleMonthGrid.Build(may2026);

        Assert.Equal(new DateOnly(2026, 4, 27), grid.FromDate);
        Assert.Equal(new DateOnly(2026, 5, 31), grid.ToDate);
    }

    // --- March 2026: starts on Sunday → 6-row grid ---

    [Fact]
    public void Return_Six_Row_Grid_When_Month_Starts_Late_In_Week()
    {
        var march2026 = new DateOnly(2026, 3, 1);

        var grid = VisibleMonthGrid.Build(march2026);

        Assert.Equal(new DateOnly(2026, 2, 23), grid.FromDate);
        Assert.Equal(new DateOnly(2026, 4, 5), grid.ToDate);
        Assert.Equal(42, grid.Days.Count); // 6 rows × 7 days
    }

    // --- February 2028: leap year (29 days) ---

    [Fact]
    public void Return_Correct_Window_For_Leap_Year_February()
    {
        var feb2028 = new DateOnly(2028, 2, 1);

        var grid = VisibleMonthGrid.Build(feb2028);

        Assert.Equal(new DateOnly(2028, 1, 31), grid.FromDate);
        Assert.Equal(new DateOnly(2028, 3, 5), grid.ToDate);
        // Feb 29 (leap day) falls in month
        Assert.Contains(grid.Days, day => day.Date == new DateOnly(2028, 2, 29) && day.IsInMonth);
    }

    // --- Days list: IsInMonth flags and contiguous coverage ---

    [Fact]
    public void Return_Days_Covering_Every_Date_In_Window_With_Correct_IsInMonth_Flags()
    {
        // June 2026: fromDate=2026-06-01, toDate=2026-07-05 → 35 days
        var june2026 = new DateOnly(2026, 6, 1);

        var grid = VisibleMonthGrid.Build(june2026);

        Assert.Equal(35, grid.Days.Count);
        // All June days are in-month
        Assert.True(grid.Days.Where(day => day.Date.Month == 6).All(day => day.IsInMonth));
        // July 1–5 are out-of-month
        Assert.True(grid.Days.Where(day => day.Date.Month == 7).All(day => !day.IsInMonth));
        // Days are contiguous
        for (var i = 1; i < grid.Days.Count; i++)
            Assert.Equal(1, grid.Days[i].Date.DayNumber - grid.Days[i - 1].Date.DayNumber);
    }
}
