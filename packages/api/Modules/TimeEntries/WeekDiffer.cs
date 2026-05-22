namespace Api.Modules.TimeEntries;

public static class WeekDiffer
{
    public static (IReadOnlyList<WeekCell> ToUpsert, IReadOnlyList<Guid> ToDelete) Diff(
        IEnumerable<TimeEntry> existingEntries,
        IEnumerable<WeekCell> incomingCells)
    {
        var existing = existingEntries.ToList();
        var nonZeroIncoming = incomingCells.Where(cell => cell.Hours > 0).ToList();

        var existingByKey = existing.ToDictionary(entry => (entry.ContractTaskId, entry.LeaveTypeId, entry.Date));
        var incomingKeys = nonZeroIncoming
            .Select(cell => (cell.ContractTaskId, cell.LeaveTypeId, cell.Date))
            .ToHashSet();

        var toDelete = existing
            .Where(entry => !incomingKeys.Contains((entry.ContractTaskId, entry.LeaveTypeId, entry.Date)))
            .Select(entry => entry.Id)
            .ToList();

        var toUpsert = nonZeroIncoming
            .Where(cell =>
            {
                var key = (cell.ContractTaskId, cell.LeaveTypeId, cell.Date);
                return !existingByKey.TryGetValue(key, out var existing) || existing.Hours != cell.Hours;
            })
            .ToList();

        return (toUpsert, toDelete);
    }
}
