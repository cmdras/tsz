namespace Api.Modules.TimeEntries;

public static class PreviousWeekAggregator
{
    public static WeekPreviousSummaryResponse Aggregate(IReadOnlyList<TimeEntry> entries)
    {
        var allChips = entries
            .Where(entry => entry.ContractTask is not null)
            .GroupBy(entry => $"{entry.ContractTask!.Contract.Customer.Name} · {entry.ContractTask.Contract.Subject}")
            .Select(group => new WeekChipResponse(group.Key, group.Sum(entry => entry.Hours)))
            .OrderByDescending(chip => chip.Hours)
            .ThenBy(chip => chip.Label, StringComparer.Ordinal)
            .ToList();

        const int maxChips = 5;
        if (allChips.Count <= maxChips)
            return new WeekPreviousSummaryResponse(allChips, null);

        var topChips = allChips.Take(maxChips).ToList();
        var remainingChips = allChips.Skip(maxChips).ToList();
        var remainingCount = remainingChips.Count;
        var remainingHours = remainingChips.Sum(chip => chip.Hours);
        var overflow = $"+{remainingCount} more · {FormatHours(remainingHours)}h";

        return new WeekPreviousSummaryResponse(topChips, overflow);
    }

    private static string FormatHours(decimal hours) =>
        hours.ToString("0.##");
}
