namespace Api.Modules.TimeEntries;

public class TimeEntryService
{
    private readonly ITimeEntryRepository _repository;

    public TimeEntryService(ITimeEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeekResponse> GetWeekAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        var weekData = await _repository.GetWeekAsync(userId, weekStart, cancellationToken);
        return new WeekResponse(
            WeekStart: weekStart,
            IsSubmitted: weekData.IsSubmitted,
            SubmittedAt: weekData.SubmittedAt,
            LastSavedAt: weekData.LastSavedAt,
            Rows: [],
            PreviousWeekSummary: new WeekPreviousSummaryResponse([], null));
    }
}
