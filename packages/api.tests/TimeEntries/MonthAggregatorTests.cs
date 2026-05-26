using Api.Modules.TimeEntries;

namespace Api.Tests.TimeEntries;

public class MonthAggregatorShould
{
    private static readonly DateOnly June2026 = new(2026, 6, 1);

    // --- Tracer bullet: empty entries → days with zero hours ---

    [Fact]
    public void Given_EmptyEntries_When_Building_Then_EachDayHasZeroHoursAndEmptyEntries()
    {
        var grid = VisibleMonthGrid.Build(June2026);

        var days = MonthAggregator.Build(grid, []);

        Assert.Equal(grid.Days.Count, days.Count);
        Assert.All(days, day =>
        {
            Assert.Equal(0m, day.TotalHours);
            Assert.Empty(day.Entries);
        });
    }

    // --- IsInMonth flags match the grid ---

    [Fact]
    public void Given_EmptyEntries_When_Building_Then_IsInMonthFlagsMatchGrid()
    {
        // June 2026: fromDate=2026-06-01 (in-month), toDate includes July days (out-of-month)
        var grid = VisibleMonthGrid.Build(June2026);

        var days = MonthAggregator.Build(grid, []);

        // All June days are in-month
        Assert.True(days.Where(day => day.Date.Month == 6).All(day => day.IsInMonth));
        // July overflow days are out-of-month
        Assert.True(days.Where(day => day.Date.Month == 7).All(day => !day.IsInMonth));
    }
}
