namespace Api.Modules.TimeEntries;

public static class MonthAggregator
{
    public static IReadOnlyList<MonthDayResponse> Build(
        VisibleMonthGridResult grid,
        IReadOnlyList<TimeEntry> entries)
    {
        var entriesByDate = entries
            .GroupBy(entry => entry.Date)
            .ToDictionary(group => group.Key, group => group.ToList());

        return grid.Days
            .Select(cell =>
            {
                if (!entriesByDate.TryGetValue(cell.Date, out var dayEntries))
                    dayEntries = [];

                var monthEntries = dayEntries
                    .Select(BuildMonthEntry)
                    .ToList();

                var totalHours = monthEntries.Sum(entry => entry.Hours);

                return new MonthDayResponse(
                    Date: cell.Date,
                    IsInMonth: cell.IsInMonth,
                    TotalHours: totalHours,
                    Entries: monthEntries);
            })
            .ToList();
    }

    private static MonthEntryResponse BuildMonthEntry(TimeEntry entry)
    {
        if (entry.ContractTask is not null)
        {
            return new MonthEntryResponse(
                Id: entry.Id,
                Kind: "task",
                Hours: entry.Hours,
                ContractTaskId: entry.ContractTaskId,
                CustomerName: entry.ContractTask.Contract.Customer.Name,
                ContractSubject: entry.ContractTask.Contract.Subject,
                TaskName: entry.ContractTask.Name,
                LeaveTypeId: null,
                LeaveTypeName: null);
        }

        return new MonthEntryResponse(
            Id: entry.Id,
            Kind: "leave",
            Hours: entry.Hours,
            ContractTaskId: null,
            CustomerName: null,
            ContractSubject: null,
            TaskName: null,
            LeaveTypeId: entry.LeaveTypeId,
            LeaveTypeName: entry.LeaveType?.Name);
    }
}
