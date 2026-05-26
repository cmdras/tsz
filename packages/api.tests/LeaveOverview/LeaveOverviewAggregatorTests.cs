using Api.Modules.LeaveOverview;
using Api.Modules.LeaveTypes;
using Api.Modules.TimeEntries;
using Api.Modules.UserLeaveAllowances;

namespace Api.Tests.LeaveOverview;

public class LeaveOverviewAggregatorShould
{
    private const int TestYear = 2026;

    private static LeaveType MakeLeaveType(string name, bool isArchived = false) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            DefaultDays = 20m,
            DefaultMode = AllowanceMode.Limited,
            IsArchived = isArchived,
        };

    private static UserLeaveAllowance MakeAllowance(Guid userId, Guid leaveTypeId, int year, decimal totalDays, AllowanceMode mode = AllowanceMode.Limited) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeaveTypeId = leaveTypeId,
            Year = year,
            Mode = mode,
            TotalDays = totalDays,
        };

    private static TimeEntry MakeLeaveEntry(Guid userId, DateOnly date, Guid leaveTypeId, decimal hours) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = date,
            LeaveTypeId = leaveTypeId,
            Hours = hours,
            UpdatedAt = DateTime.UtcNow,
        };

    // --- Tracer bullet: empty year returns empty days, zero types ---

    [Fact]
    public void Given_EmptyYear_When_Aggregating_Then_ReturnsEmptyDaysAndZeroedTypes()
    {
        var userId = Guid.NewGuid();
        var leaveType = MakeLeaveType("Annual Leave");
        var allowance = MakeAllowance(userId, leaveType.Id, TestYear, 20m);

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [allowance], []);

        Assert.Equal(TestYear, result.Year);
        Assert.Empty(result.Days);
        Assert.Single(result.Types);
        Assert.Equal(0m, result.Types[0].TakenDays);
    }

    // --- Limited + Unlimited mix both appear ---

    [Fact]
    public void Given_LimitedAndUnlimitedTypes_When_Aggregating_Then_BothAppearWithCorrectMode()
    {
        var userId = Guid.NewGuid();
        var limited = MakeLeaveType("Annual Leave");
        var unlimited = MakeLeaveType("Sick Leave");
        unlimited.DefaultMode = AllowanceMode.Unlimited;
        var allowanceLimited = MakeAllowance(userId, limited.Id, TestYear, 20m, AllowanceMode.Limited);
        var allowanceUnlimited = MakeAllowance(userId, unlimited.Id, TestYear, 0m, AllowanceMode.Unlimited);

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [limited, unlimited], [allowanceLimited, allowanceUnlimited], []);

        Assert.Equal(2, result.Types.Count);
        var limitedItem = result.Types.Single(type => type.Id == limited.Id);
        var unlimitedItem = result.Types.Single(type => type.Id == unlimited.Id);
        Assert.Equal("Limited", limitedItem.Mode);
        Assert.Equal("Unlimited", unlimitedItem.Mode);
    }

    // --- Zero-allowance Limited type still appears ---

    [Fact]
    public void Given_LimitedTypeWithNoAllowanceRow_When_Aggregating_Then_TypeAppearsWithZeroAllowance()
    {
        var leaveType = MakeLeaveType("Bereavement");

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [], []);

        var typeItem = Assert.Single(result.Types);
        Assert.Equal(0m, typeItem.Allowance);
        Assert.Equal("Limited", typeItem.Mode);
    }

    // --- Partial day (4h) contributes 0.5 to takenDays ---

    [Fact]
    public void Given_FourHourLeaveEntry_When_Aggregating_Then_TakenDaysIsHalf()
    {
        var userId = Guid.NewGuid();
        var leaveType = MakeLeaveType("Annual Leave");
        var allowance = MakeAllowance(userId, leaveType.Id, TestYear, 20m);
        var entry = MakeLeaveEntry(userId, new DateOnly(TestYear, 6, 1), leaveType.Id, 4m);

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [allowance], [entry]);

        var typeItem = result.Types.Single();
        Assert.Equal(0.5m, typeItem.TakenDays);
    }

    // --- Over-allocated user returned as-is ---

    [Fact]
    public void Given_UserTakenMoreThanAllowance_When_Aggregating_Then_TakenDaysExceedsAllowance()
    {
        var userId = Guid.NewGuid();
        var leaveType = MakeLeaveType("Annual Leave");
        var allowance = MakeAllowance(userId, leaveType.Id, TestYear, 5m);
        var entries = new[]
        {
            MakeLeaveEntry(userId, new DateOnly(TestYear, 1, 2), leaveType.Id, 8m),
            MakeLeaveEntry(userId, new DateOnly(TestYear, 1, 3), leaveType.Id, 8m),
            MakeLeaveEntry(userId, new DateOnly(TestYear, 1, 4), leaveType.Id, 8m),
            MakeLeaveEntry(userId, new DateOnly(TestYear, 1, 5), leaveType.Id, 8m),
            MakeLeaveEntry(userId, new DateOnly(TestYear, 1, 6), leaveType.Id, 8m),
            MakeLeaveEntry(userId, new DateOnly(TestYear, 1, 9), leaveType.Id, 8m),
        };

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [allowance], entries);

        var typeItem = result.Types.Single();
        Assert.Equal(6m, typeItem.TakenDays);
        Assert.Equal(5m, typeItem.Allowance);
    }

    // --- days[] only contains dates with hours > 0 ---

    [Fact]
    public void Given_ZeroHourEntry_When_Aggregating_Then_DayNotIncluded()
    {
        var userId = Guid.NewGuid();
        var leaveType = MakeLeaveType("Annual Leave");
        var entry = MakeLeaveEntry(userId, new DateOnly(TestYear, 3, 15), leaveType.Id, 0m);

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [], [entry]);

        Assert.Empty(result.Days);
    }

    // --- sort order: types alphabetical, days ascending ---

    [Fact]
    public void Given_UnsortedTypesAndDays_When_Aggregating_Then_TypesAlphabeticalAndDaysAscending()
    {
        var userId = Guid.NewGuid();
        var typeZebra = MakeLeaveType("Zebra Leave");
        var typeApple = MakeLeaveType("Apple Leave");
        var typeMango = MakeLeaveType("Mango Leave");
        var laterDate = new DateOnly(TestYear, 6, 15);
        var earlierDate = new DateOnly(TestYear, 3, 1);
        var entries = new[]
        {
            MakeLeaveEntry(userId, laterDate, typeZebra.Id, 8m),
            MakeLeaveEntry(userId, earlierDate, typeApple.Id, 8m),
        };

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [typeZebra, typeApple, typeMango], [], entries);

        Assert.Equal("Apple Leave", result.Types[0].Name);
        Assert.Equal("Mango Leave", result.Types[1].Name);
        Assert.Equal("Zebra Leave", result.Types[2].Name);
        Assert.Equal(earlierDate, result.Days[0].Date);
        Assert.Equal(laterDate, result.Days[1].Date);
    }

    // --- Archived leave types excluded ---

    [Fact]
    public void Given_ArchivedLeaveType_When_Aggregating_Then_ExcludedFromTypes()
    {
        var leaveType = MakeLeaveType("Archived Leave", isArchived: true);

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [], []);

        Assert.Empty(result.Types);
    }

    // --- Day contains distinct leaveTypeIds for that date ---

    [Fact]
    public void Given_TwoLeaveEntriesOnSameDay_When_Aggregating_Then_DayHasBothLeaveTypeIds()
    {
        var userId = Guid.NewGuid();
        var leaveType1 = MakeLeaveType("Annual Leave");
        var leaveType2 = MakeLeaveType("Sick Leave");
        var date = new DateOnly(TestYear, 5, 4);
        var entries = new[]
        {
            MakeLeaveEntry(userId, date, leaveType1.Id, 4m),
            MakeLeaveEntry(userId, date, leaveType2.Id, 4m),
        };

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType1, leaveType2], [], entries);

        var day = Assert.Single(result.Days);
        Assert.Equal(date, day.Date);
        Assert.Contains(leaveType1.Id, day.LeaveTypeIds);
        Assert.Contains(leaveType2.Id, day.LeaveTypeIds);
    }

    // --- Entries from other years not included in days ---

    [Fact]
    public void Given_EntryFromDifferentYear_When_Aggregating_Then_DayNotIncluded()
    {
        var userId = Guid.NewGuid();
        var leaveType = MakeLeaveType("Annual Leave");
        var entry = MakeLeaveEntry(userId, new DateOnly(TestYear - 1, 12, 31), leaveType.Id, 8m);

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [], [entry]);

        Assert.Empty(result.Days);
    }

    // --- takenDays only counts entries for the given year ---

    [Fact]
    public void Given_EntriesFromMultipleYears_When_Aggregating_Then_TakenDaysOnlyCountsGivenYear()
    {
        var userId = Guid.NewGuid();
        var leaveType = MakeLeaveType("Annual Leave");
        var allowance = MakeAllowance(userId, leaveType.Id, TestYear, 20m);
        var entries = new[]
        {
            MakeLeaveEntry(userId, new DateOnly(TestYear, 3, 1), leaveType.Id, 8m),
            MakeLeaveEntry(userId, new DateOnly(TestYear - 1, 12, 15), leaveType.Id, 8m),
        };

        var result = LeaveOverviewAggregator.Aggregate(TestYear, [leaveType], [allowance], entries);

        var typeItem = result.Types.Single();
        Assert.Equal(1m, typeItem.TakenDays);
    }
}
