using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.TimeEntries;
using Api.Modules.UserLeaveAllowances;

namespace Api.Tests.TimeEntries;

public class WeekSchedulerShould
{
    private static readonly DateOnly Week = new(2026, 5, 18);

    private static Contract MakeContract(
        Guid consultantId,
        string customerName = "Acme",
        string subject = "Project X",
        bool isArchived = false,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        IEnumerable<ContractTask>? tasks = null)
    {
        return new Contract
        {
            Id = Guid.NewGuid(),
            ConsultantId = consultantId,
            Customer = new Customer { Id = Guid.NewGuid(), Name = customerName },
            Subject = subject,
            IsArchived = isArchived,
            StartDate = startDate ?? new DateOnly(2026, 1, 1),
            EndDate = endDate,
            Tasks = (tasks ?? [MakeTask()]).ToList(),
        };
    }

    private static ContractTask MakeTask(string name = "Development", bool isArchived = false) =>
        new() { Id = Guid.NewGuid(), Name = name, IsArchived = isArchived };

    private static LeaveType MakeLeaveType(string name = "Annual Leave") =>
        new() { Id = Guid.NewGuid(), Name = name, DefaultDays = 20, DefaultMode = AllowanceMode.Limited };

    [Fact]
    public void Exclude_Contract_Belonging_To_Different_Consultant()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var contracts = new[] { MakeContract(otherUserId) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Empty(result.AvailableTasks);
    }

    [Fact]
    public void Exclude_Archived_Contract()
    {
        var userId = Guid.NewGuid();
        var contracts = new[] { MakeContract(userId, isArchived: true) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Empty(result.AvailableTasks);
    }

    [Fact]
    public void Exclude_Contract_Ending_Before_Week_Starts()
    {
        var userId = Guid.NewGuid();
        var contracts = new[] { MakeContract(userId, endDate: new DateOnly(2026, 5, 10)) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Empty(result.AvailableTasks);
    }

    [Fact]
    public void Exclude_Contract_Starting_After_Week_Ends()
    {
        var userId = Guid.NewGuid();
        var contracts = new[] { MakeContract(userId, startDate: new DateOnly(2026, 5, 25)) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Empty(result.AvailableTasks);
    }

    [Fact]
    public void Exclude_Archived_Task()
    {
        var userId = Guid.NewGuid();
        var contracts = new[] { MakeContract(userId, tasks: [MakeTask(isArchived: true)]) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Empty(result.AvailableTasks);
    }

    [Fact]
    public void Exclude_Task_Already_On_Grid()
    {
        var userId = Guid.NewGuid();
        var task = MakeTask();
        var contracts = new[] { MakeContract(userId, tasks: [task]) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], [task.Id]);

        Assert.Empty(result.AvailableTasks);
    }

    [Fact]
    public void Include_Valid_Task_With_Correct_Labels()
    {
        var userId = Guid.NewGuid();
        var task = MakeTask("Analysis");
        var contracts = new[] { MakeContract(userId, customerName: "Globex", subject: "Cloud Migration", tasks: [task]) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Single(result.AvailableTasks);
        var option = result.AvailableTasks[0];
        Assert.Equal(task.Id, option.ContractTaskId);
        Assert.Equal("Globex", option.CustomerName);
        Assert.Equal("Cloud Migration", option.ContractSubject);
        Assert.Equal("Analysis", option.TaskName);
    }

    [Fact]
    public void Include_Contract_With_No_End_Date()
    {
        var userId = Guid.NewGuid();
        var contracts = new[] { MakeContract(userId, endDate: null) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Single(result.AvailableTasks);
    }

    [Fact]
    public void Include_Contract_Whose_End_Date_Falls_Within_Week()
    {
        var userId = Guid.NewGuid();
        var contracts = new[] { MakeContract(userId, endDate: new DateOnly(2026, 5, 20)) };

        var result = WeekScheduler.BuildPickerOptions(userId, Week, contracts, [], []);

        Assert.Single(result.AvailableTasks);
    }

    [Fact]
    public void Return_Leave_Types_As_Is()
    {
        var userId = Guid.NewGuid();
        var leaveType = MakeLeaveType("Sick Leave");

        var result = WeekScheduler.BuildPickerOptions(userId, Week, [], [leaveType], []);

        Assert.Single(result.AvailableLeaveTypes);
        Assert.Equal(leaveType.Id, result.AvailableLeaveTypes[0].LeaveTypeId);
        Assert.Equal("Sick Leave", result.AvailableLeaveTypes[0].Name);
    }
}
