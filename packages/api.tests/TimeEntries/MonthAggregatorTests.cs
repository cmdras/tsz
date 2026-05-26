using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.TimeEntries;

namespace Api.Tests.TimeEntries;

public class MonthAggregatorShould
{
    private static readonly DateOnly June2026 = new(2026, 6, 1);

    private static Customer MakeCustomer(string name) =>
        new() { Id = Guid.NewGuid(), Name = name, Number = 1 };

    private static ContractTask MakeContractTask(Customer customer, string contractSubject, string taskName)
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Number = 1,
            CustomerId = customer.Id,
            Customer = customer,
            Subject = contractSubject,
            ConsultantId = Guid.NewGuid(),
            StartDate = new DateOnly(2026, 1, 1),
        };
        return new ContractTask
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            Contract = contract,
            Name = taskName,
        };
    }

    private static LeaveType MakeLeaveType(string name) =>
        new() { Id = Guid.NewGuid(), Name = name };

    private static TimeEntry MakeTaskEntry(DateOnly date, ContractTask contractTask, decimal hours) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = date,
            ContractTaskId = contractTask.Id,
            ContractTask = contractTask,
            Hours = hours,
            UpdatedAt = DateTime.UtcNow,
        };

    private static TimeEntry MakeLeaveEntry(DateOnly date, LeaveType leaveType, decimal hours) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = date,
            LeaveTypeId = leaveType.Id,
            LeaveType = leaveType,
            Hours = hours,
            UpdatedAt = DateTime.UtcNow,
        };

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

    // --- Single task entry on a single day ---

    [Fact]
    public void Given_SingleTaskEntry_When_Building_Then_DayHasCorrectEntryAndTotalHours()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var customer = MakeCustomer("Acme");
        var task = MakeContractTask(customer, "Build App", "Development");
        var monday = new DateOnly(2026, 6, 1);
        var entry = MakeTaskEntry(monday, task, 8m);

        var days = MonthAggregator.Build(grid, [entry]);

        var day = days.Single(day => day.Date == monday);
        Assert.Equal(8m, day.TotalHours);
        Assert.Single(day.Entries);
        var monthEntry = day.Entries[0];
        Assert.Equal("task", monthEntry.Kind);
        Assert.Equal(8m, monthEntry.Hours);
        Assert.Equal(task.Id, monthEntry.ContractTaskId);
        Assert.Equal("Acme", monthEntry.CustomerName);
        Assert.Equal("Build App", monthEntry.ContractSubject);
        Assert.Equal("Development", monthEntry.TaskName);
        Assert.Null(monthEntry.LeaveTypeId);
        Assert.Null(monthEntry.LeaveTypeName);
    }

    // --- Single leave entry on a single day ---

    [Fact]
    public void Given_SingleLeaveEntry_When_Building_Then_DayHasCorrectEntryAndTotalHours()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var leaveType = MakeLeaveType("Annual Leave");
        var monday = new DateOnly(2026, 6, 1);
        var entry = MakeLeaveEntry(monday, leaveType, 8m);

        var days = MonthAggregator.Build(grid, [entry]);

        var day = days.Single(day => day.Date == monday);
        Assert.Equal(8m, day.TotalHours);
        Assert.Single(day.Entries);
        var monthEntry = day.Entries[0];
        Assert.Equal("leave", monthEntry.Kind);
        Assert.Equal(8m, monthEntry.Hours);
        Assert.Null(monthEntry.ContractTaskId);
        Assert.Null(monthEntry.CustomerName);
        Assert.Null(monthEntry.ContractSubject);
        Assert.Null(monthEntry.TaskName);
        Assert.Equal(leaveType.Id, monthEntry.LeaveTypeId);
        Assert.Equal("Annual Leave", monthEntry.LeaveTypeName);
    }

    // --- Multi-day single contract: entries land on correct days ---

    [Fact]
    public void Given_MultiDaySingleContract_When_Building_Then_EntriesLandOnCorrectDays()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var customer = MakeCustomer("Acme");
        var task = MakeContractTask(customer, "Build App", "Development");
        var monday = new DateOnly(2026, 6, 1);
        var tuesday = monday.AddDays(1);
        var entries = new[]
        {
            MakeTaskEntry(monday, task, 8m),
            MakeTaskEntry(tuesday, task, 6m),
        };

        var days = MonthAggregator.Build(grid, entries);

        var mondayDay = days.Single(day => day.Date == monday);
        var tuesdayDay = days.Single(day => day.Date == tuesday);
        Assert.Equal(8m, mondayDay.TotalHours);
        Assert.Single(mondayDay.Entries);
        Assert.Equal(6m, tuesdayDay.TotalHours);
        Assert.Single(tuesdayDay.Entries);
    }

    // --- Multi-contract single day: totalHours sums all entries ---

    [Fact]
    public void Given_MultiContractSingleDay_When_Building_Then_TotalHoursSumsAllEntries()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var customerA = MakeCustomer("Acme");
        var customerB = MakeCustomer("Globex");
        var taskA = MakeContractTask(customerA, "Build App", "Development");
        var taskB = MakeContractTask(customerB, "Consulting", "Architecture");
        var monday = new DateOnly(2026, 6, 1);
        var entries = new[]
        {
            MakeTaskEntry(monday, taskA, 4m),
            MakeTaskEntry(monday, taskB, 4m),
        };

        var days = MonthAggregator.Build(grid, entries);

        var day = days.Single(day => day.Date == monday);
        Assert.Equal(8m, day.TotalHours);
        Assert.Equal(2, day.Entries.Count);
    }

    // --- Leave + task on same day: both appear and totalHours includes both ---

    [Fact]
    public void Given_LeaveAndTaskOnSameDay_When_Building_Then_TotalHoursIncludesBoth()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var customer = MakeCustomer("Acme");
        var task = MakeContractTask(customer, "Build App", "Development");
        var leaveType = MakeLeaveType("Sick Leave");
        var monday = new DateOnly(2026, 6, 1);
        var entries = new[]
        {
            MakeTaskEntry(monday, task, 4m),
            MakeLeaveEntry(monday, leaveType, 4m),
        };

        var days = MonthAggregator.Build(grid, entries);

        var day = days.Single(day => day.Date == monday);
        Assert.Equal(8m, day.TotalHours);
        Assert.Equal(2, day.Entries.Count);
        Assert.Contains(day.Entries, entry => entry.Kind == "task");
        Assert.Contains(day.Entries, entry => entry.Kind == "leave");
    }

    // --- Out-of-month days also get their entries populated ---

    [Fact]
    public void Given_EntryOnOutOfMonthDay_When_Building_Then_DayHasEntryWithIsInMonthFalse()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var customer = MakeCustomer("Acme");
        var task = MakeContractTask(customer, "Build App", "Development");
        // July 5 is in the visible window but out-of-month for June 2026
        var july5 = new DateOnly(2026, 7, 5);
        var entry = MakeTaskEntry(july5, task, 7m);

        var days = MonthAggregator.Build(grid, [entry]);

        var day = days.Single(day => day.Date == july5);
        Assert.False(day.IsInMonth);
        Assert.Equal(7m, day.TotalHours);
        Assert.Single(day.Entries);
        Assert.Equal("task", day.Entries[0].Kind);
    }

    // --- Days without entries remain at zero ---

    [Fact]
    public void Given_EntryOnOneDay_When_Building_Then_OtherDaysRemainAtZero()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var customer = MakeCustomer("Acme");
        var task = MakeContractTask(customer, "Build App", "Development");
        var monday = new DateOnly(2026, 6, 1);
        var entry = MakeTaskEntry(monday, task, 8m);

        var days = MonthAggregator.Build(grid, [entry]);

        var otherDays = days.Where(day => day.Date != monday);
        Assert.All(otherDays, day =>
        {
            Assert.Equal(0m, day.TotalHours);
            Assert.Empty(day.Entries);
        });
    }

    // --- Denormalized names from nav properties ---

    [Fact]
    public void Given_TaskEntry_When_Building_Then_DenormalizedNamesComesFromNavProperties()
    {
        var grid = VisibleMonthGrid.Build(June2026);
        var customer = MakeCustomer("Globex Corp");
        var task = MakeContractTask(customer, "Platform Modernization", "Backend");
        var monday = new DateOnly(2026, 6, 1);
        var entry = MakeTaskEntry(monday, task, 5m);

        var days = MonthAggregator.Build(grid, [entry]);

        var monthEntry = days.Single(day => day.Date == monday).Entries[0];
        Assert.Equal("Globex Corp", monthEntry.CustomerName);
        Assert.Equal("Platform Modernization", monthEntry.ContractSubject);
        Assert.Equal("Backend", monthEntry.TaskName);
    }
}
