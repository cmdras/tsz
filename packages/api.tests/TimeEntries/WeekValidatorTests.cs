using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.TimeEntries;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;

namespace Api.Tests.TimeEntries;

public class WeekValidatorShould
{
    private static readonly DateOnly Monday = new(2026, 5, 18);
    private static readonly DateOnly Tuesday = Monday.AddDays(1);
    private static readonly DateOnly Saturday = Monday.AddDays(5);
    private static readonly DateOnly Sunday = Monday.AddDays(6);
    private static readonly Guid UserId = Guid.NewGuid();

    private static Contract MakeContract(
        Guid userId,
        Guid taskId,
        bool taskArchived = false,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool contractArchived = false) => new()
    {
        Id = Guid.NewGuid(),
        ConsultantId = userId,
        StartDate = startDate ?? Monday,
        EndDate = endDate,
        IsArchived = contractArchived,
        Tasks = [new ContractTask { Id = taskId, Name = "Task", IsArchived = taskArchived }],
        Customer = new Customer { Id = Guid.NewGuid(), Name = "Cust" },
        Consultant = new User { Id = userId, Name = "User" },
    };

    private static LeaveType MakeLeaveType(bool isArchived = false) =>
        new() { Id = Guid.NewGuid(), Name = "Annual Leave", DefaultDays = 20, DefaultMode = AllowanceMode.Limited, IsArchived = isArchived };

    private static WeekCell MakeCell(DateOnly date, Guid taskId, decimal hours) =>
        new(ContractTaskId: taskId, LeaveTypeId: null, Date: date, Hours: hours);

    private static WeekCell MakeLeaveCell(DateOnly date, Guid leaveTypeId, decimal hours) =>
        new(ContractTaskId: null, LeaveTypeId: leaveTypeId, Date: date, Hours: hours);

    // --- Valid inputs ---

    [Fact]
    public void Return_Valid_For_Half_Hour_Grain()
    {
        var taskId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var cells = new[] { MakeCell(Monday, taskId, 0.5m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Return_Valid_For_Full_Day()
    {
        var taskId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var cells = new[] { MakeCell(Monday, taskId, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.True(result.IsValid);
    }

    // --- Hours validation ---

    [Theory]
    [InlineData(0.25)]
    [InlineData(1.3)]
    [InlineData(7.1)]
    public void Reject_Off_Grain_Hours(double hours)
    {
        var taskId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var cells = new[] { MakeCell(Monday, taskId, (decimal)hours) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Reject_Hours_Above_24()
    {
        var taskId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var cells = new[] { MakeCell(Monday, taskId, 24.5m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Reject_Zero_Hours()
    {
        var taskId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var cells = new[] { MakeCell(Monday, taskId, 0m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    // --- Weekend check ---

    [Theory]
    [InlineData(5)] // Saturday
    [InlineData(6)] // Sunday
    public void Reject_Weekend_Cells(int daysOffset)
    {
        var taskId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var date = Monday.AddDays(daysOffset);
        var cells = new[] { MakeCell(date, taskId, 4m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    // --- Daily sum ---

    [Fact]
    public void Reject_Daily_Sum_Over_24_Across_Multiple_Tasks()
    {
        var taskA = Guid.NewGuid();
        var taskB = Guid.NewGuid();
        var contracts = new[]
        {
            MakeContract(UserId, taskA),
            MakeContract(UserId, taskB),
        };
        var cells = new[]
        {
            MakeCell(Monday, taskA, 16m),
            MakeCell(Monday, taskB, 12m),
        };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Accept_Daily_Sum_Exactly_24()
    {
        var taskA = Guid.NewGuid();
        var taskB = Guid.NewGuid();
        var contracts = new[]
        {
            MakeContract(UserId, taskA),
            MakeContract(UserId, taskB),
        };
        var cells = new[]
        {
            MakeCell(Monday, taskA, 12m),
            MakeCell(Monday, taskB, 12m),
        };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.True(result.IsValid);
    }

    // --- FK exclusivity ---

    [Fact]
    public void Reject_Cell_With_Both_FKs_Set()
    {
        var taskId = Guid.NewGuid();
        var leaveId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var cell = new WeekCell(ContractTaskId: taskId, LeaveTypeId: leaveId, Date: Monday, Hours: 8m);

        var result = WeekValidator.Validate([cell], Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Reject_Cell_With_Neither_FK_Set()
    {
        var contracts = Array.Empty<Contract>();
        var cell = new WeekCell(ContractTaskId: null, LeaveTypeId: null, Date: Monday, Hours: 8m);

        var result = WeekValidator.Validate([cell], Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    // --- Contract ownership ---

    [Fact]
    public void Reject_Task_From_Another_Users_Contract()
    {
        var taskId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var contracts = new[] { MakeContract(otherUserId, taskId) };
        var cells = new[] { MakeCell(Monday, taskId, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    // --- Contract date intersection ---

    [Fact]
    public void Reject_Task_Whose_Contract_Does_Not_Intersect_Week()
    {
        var taskId = Guid.NewGuid();
        var nextMonday = Monday.AddDays(7);
        var contracts = new[] { MakeContract(UserId, taskId, startDate: nextMonday) };
        var cells = new[] { MakeCell(Monday, taskId, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Reject_Task_Whose_Contract_Ended_Before_Week()
    {
        var taskId = Guid.NewGuid();
        var lastWeekFriday = Monday.AddDays(-3);
        var contracts = new[] { MakeContract(UserId, taskId, endDate: lastWeekFriday) };
        var cells = new[] { MakeCell(Monday, taskId, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    // --- Archived task ---

    [Fact]
    public void Reject_Archived_Task()
    {
        var taskId = Guid.NewGuid();
        var contracts = new[] { MakeContract(UserId, taskId, taskArchived: true) };
        var cells = new[] { MakeCell(Monday, taskId, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts);

        Assert.False(result.IsValid);
    }

    // --- Leave type FK ---

    [Fact]
    public void Return_Valid_For_Leave_Cell()
    {
        var leaveType = MakeLeaveType();
        var cells = new[] { MakeLeaveCell(Monday, leaveType.Id, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, [], [leaveType]);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Reject_Leave_Cell_With_Unknown_LeaveTypeId()
    {
        var unknownId = Guid.NewGuid();
        var cells = new[] { MakeLeaveCell(Monday, unknownId, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, [], []);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Reject_Leave_Cell_With_Archived_LeaveType()
    {
        var leaveType = MakeLeaveType(isArchived: true);
        var cells = new[] { MakeLeaveCell(Monday, leaveType.Id, 8m) };

        var result = WeekValidator.Validate(cells, Monday, UserId, [], [leaveType]);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Accept_Mixed_Task_And_Leave_Cells_In_Same_Week()
    {
        var taskId = Guid.NewGuid();
        var leaveType = MakeLeaveType();
        var contracts = new[] { MakeContract(UserId, taskId) };
        var cells = new[]
        {
            MakeCell(Monday, taskId, 8m),
            MakeLeaveCell(Tuesday, leaveType.Id, 8m),
        };

        var result = WeekValidator.Validate(cells, Monday, UserId, contracts, [leaveType]);

        Assert.True(result.IsValid);
    }
}
