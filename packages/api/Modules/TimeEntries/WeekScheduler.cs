using Api.Modules.Contracts;
using Api.Modules.LeaveTypes;

namespace Api.Modules.TimeEntries;

public static class WeekScheduler
{
    public static PickerOptions BuildPickerOptions(
        Guid userId,
        DateOnly weekStart,
        IEnumerable<Contract> contracts,
        IEnumerable<LeaveType> leaveTypes,
        IEnumerable<Guid> alreadyOnGrid)
    {
        var weekEnd = weekStart.AddDays(6);
        var alreadyOnGridSet = alreadyOnGrid.ToHashSet();

        var availableTasks = contracts
            .Where(contract =>
                contract.ConsultantId == userId
                && !contract.IsArchived
                && contract.StartDate <= weekEnd
                && (contract.EndDate == null || contract.EndDate >= weekStart))
            .SelectMany(contract => contract.Tasks
                .Where(task => !task.IsArchived && !alreadyOnGridSet.Contains(task.Id))
                .Select(task => new PickerTaskOption(
                    ContractTaskId: task.Id,
                    CustomerName: contract.Customer.Name,
                    ContractSubject: contract.Subject,
                    TaskName: task.Name)))
            .ToList();

        var availableLeaveTypes = leaveTypes
            .Select(leaveType => new PickerLeaveTypeOption(leaveType.Id, leaveType.Name))
            .ToList();

        return new PickerOptions(AvailableTasks: availableTasks, AvailableLeaveTypes: availableLeaveTypes);
    }
}
