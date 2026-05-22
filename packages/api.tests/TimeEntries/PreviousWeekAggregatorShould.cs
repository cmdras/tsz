using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.TimeEntries;

namespace Api.Tests.TimeEntries;

public class PreviousWeekAggregatorShould
{
    private static TimeEntry MakeTaskEntry(string customerName, string contractSubject, decimal hours)
    {
        var task = new ContractTask { Id = Guid.NewGuid(), Name = "Dev", IsArchived = false };
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            ConsultantId = Guid.NewGuid(),
            Customer = new Customer { Id = Guid.NewGuid(), Name = customerName },
            Subject = contractSubject,
            IsArchived = false,
            StartDate = new DateOnly(2026, 1, 1),
            Tasks = [task],
        };
        task.Contract = contract;

        return new TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = new DateOnly(2026, 5, 12),
            ContractTaskId = task.Id,
            ContractTask = task,
            Hours = hours,
        };
    }

    private static TimeEntry MakeLeaveEntry(decimal hours)
    {
        var leaveType = new LeaveType { Id = Guid.NewGuid(), Name = "Annual Leave" };
        return new TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = new DateOnly(2026, 5, 12),
            LeaveTypeId = leaveType.Id,
            LeaveType = leaveType,
            Hours = hours,
        };
    }

    [Fact]
    public void Return_Empty_Chips_For_Empty_Input()
    {
        var result = PreviousWeekAggregator.Aggregate([]);

        Assert.Empty(result.Chips);
        Assert.Null(result.Overflow);
    }

    [Fact]
    public void Return_One_Chip_For_Single_Task_Entry()
    {
        var entry = MakeTaskEntry("Acme", "Project X", 8);

        var result = PreviousWeekAggregator.Aggregate([entry]);

        Assert.Single(result.Chips);
        Assert.Equal("Acme · Project X", result.Chips[0].Label);
        Assert.Equal(8, result.Chips[0].Hours);
        Assert.Null(result.Overflow);
    }

    [Fact]
    public void Return_Empty_Chips_For_Leave_Only_Entries()
    {
        var leaveEntry = MakeLeaveEntry(8);

        var result = PreviousWeekAggregator.Aggregate([leaveEntry]);

        Assert.Empty(result.Chips);
        Assert.Null(result.Overflow);
    }

    [Fact]
    public void Merge_Entries_With_Same_Customer_And_Contract()
    {
        var entry1 = MakeTaskEntry("Acme", "Project X", 6);
        var entry2 = MakeTaskEntry("Acme", "Project X", 4);

        var result = PreviousWeekAggregator.Aggregate([entry1, entry2]);

        Assert.Single(result.Chips);
        Assert.Equal("Acme · Project X", result.Chips[0].Label);
        Assert.Equal(10, result.Chips[0].Hours);
    }

    [Fact]
    public void Return_Top_5_Chips_And_Overflow_When_More_Than_5_Groups()
    {
        var entries = new[]
        {
            MakeTaskEntry("Alpha", "A", 10),
            MakeTaskEntry("Beta", "B", 8),
            MakeTaskEntry("Gamma", "C", 6),
            MakeTaskEntry("Delta", "D", 4),
            MakeTaskEntry("Epsilon", "E", 3),
            MakeTaskEntry("Zeta", "F", 2),
        };

        var result = PreviousWeekAggregator.Aggregate(entries);

        Assert.Equal(5, result.Chips.Count);
        Assert.Equal("Alpha · A", result.Chips[0].Label);
        Assert.Equal("+1 more · 2h", result.Overflow);
    }

    [Fact]
    public void Break_Ties_Alphabetically_By_Label()
    {
        var entryBravo = MakeTaskEntry("Bravo", "Z", 5);
        var entryAlpha = MakeTaskEntry("Alpha", "Z", 5);

        var result = PreviousWeekAggregator.Aggregate([entryBravo, entryAlpha]);

        Assert.Equal(2, result.Chips.Count);
        Assert.Equal("Alpha · Z", result.Chips[0].Label);
        Assert.Equal("Bravo · Z", result.Chips[1].Label);
    }
}
