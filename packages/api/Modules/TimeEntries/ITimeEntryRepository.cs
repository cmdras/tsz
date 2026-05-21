namespace Api.Modules.TimeEntries;

public interface ITimeEntryRepository
{
    Task<WeekData> GetWeekAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken = default);
}

public record WeekData(bool IsSubmitted, DateTime? SubmittedAt, DateTime? LastSavedAt);
