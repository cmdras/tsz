using Api.Common.Exceptions;

namespace Api.Modules.TimeEntries;

public class InvalidTimeEntryRequestException(string message) : DomainException(message, 400);

public class TimeEntryService
{
    private readonly ITimeEntryRepository _repository;

    public TimeEntryService(ITimeEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeekResponse> GetWeekAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        if (weekStart.DayOfWeek != DayOfWeek.Monday)
            throw new InvalidTimeEntryRequestException("weekStart must be a Monday.");

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
