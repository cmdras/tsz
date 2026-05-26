using Api.Modules.LeaveTypes;
using Api.Modules.TimeEntries;
using Api.Modules.UserLeaveAllowances;

namespace Api.Modules.LeaveOverview;

public static class LeaveOverviewAggregator
{
    public static LeaveOverviewResponse Aggregate(
        int year,
        IReadOnlyList<LeaveType> leaveTypes,
        IReadOnlyList<UserLeaveAllowance> userAllowances,
        IReadOnlyList<TimeEntry> timeEntries)
    {
        var activeLeaveTypes = leaveTypes
            .Where(leaveType => !leaveType.IsArchived)
            .ToList();

        var activeLeaveTypeIds = activeLeaveTypes.Select(leaveType => leaveType.Id).ToHashSet();

        var yearEntries = timeEntries
            .Where(entry => entry.LeaveTypeId.HasValue
                && entry.Hours > 0
                && entry.Date.Year == year
                && activeLeaveTypeIds.Contains(entry.LeaveTypeId.Value))
            .ToList();

        var allowanceByLeaveTypeId = userAllowances
            .ToDictionary(allowance => allowance.LeaveTypeId);

        var typeItems = activeLeaveTypes
            .OrderBy(leaveType => leaveType.Name)
            .Select(leaveType =>
            {
                allowanceByLeaveTypeId.TryGetValue(leaveType.Id, out var allowance);

                var takenHours = yearEntries
                    .Where(entry => entry.LeaveTypeId == leaveType.Id)
                    .Sum(entry => entry.Hours);

                var takenDays = takenHours / 8m;
                var allowanceDays = allowance?.TotalDays ?? 0m;
                var mode = allowance is not null
                    ? allowance.Mode.ToString()
                    : leaveType.DefaultMode.ToString();

                return new LeaveOverviewTypeItem(
                    Id: leaveType.Id,
                    Name: leaveType.Name,
                    Mode: mode,
                    Allowance: allowanceDays,
                    TakenDays: takenDays);
            })
            .ToList();

        var dayItems = yearEntries
            .GroupBy(entry => entry.Date)
            .OrderBy(group => group.Key)
            .Select(group => new LeaveOverviewDayItem(
                Date: group.Key,
                LeaveTypeIds: group
                    .Select(entry => entry.LeaveTypeId!.Value)
                    .Distinct()
                    .ToList()))
            .ToList();

        return new LeaveOverviewResponse(
            Year: year,
            Types: typeItems,
            Days: dayItems);
    }
}
