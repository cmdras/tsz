using Api.Common;
using Api.Common.Database;
using Api.Modules.TimeEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.TimeEntries;

file sealed class FakeClock : IClock
{
    public DateTime UtcNow { get; } = new DateTime(2026, 5, 26, 12, 0, 0, DateTimeKind.Utc);
}

public class TimeEntryServiceUnsubmitShould
{
    private static (TimeEntryService service, AppDbContext context) CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new AppDbContext(options);
        var service = new TimeEntryService(new TimeEntryRepository(context), new FakeClock());
        return (service, context);
    }

    private static async Task SeedSubmissionAsync(AppDbContext context, Guid userId, DateOnly weekStart)
    {
        context.WeekSubmissions.Add(new WeekSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WeekStart = weekStart,
            SubmittedAt = new DateTime(2026, 5, 23, 17, 0, 0, DateTimeKind.Utc),
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Given_NonMondayWeekStart_When_Unsubmitting_Then_Throws400()
    {
        var (service, _) = CreateService();
        var userId = Guid.NewGuid();
        var tuesday = new DateOnly(2026, 5, 19);

        var exception = await Assert.ThrowsAsync<InvalidTimeEntryRequestException>(
            () => service.UnsubmitWeekAsync(userId, tuesday));

        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task Given_NoSubmissionExists_When_Unsubmitting_Then_Throws404()
    {
        var (service, _) = CreateService();
        var userId = Guid.NewGuid();
        var monday = new DateOnly(2026, 5, 18);

        var exception = await Assert.ThrowsAsync<WeekNotSubmittedException>(
            () => service.UnsubmitWeekAsync(userId, monday));

        Assert.Equal(404, exception.StatusCode);
    }

    [Fact]
    public async Task Given_SubmittedWeek_When_Unsubmitting_Then_ReturnsWeekWithIsSubmittedFalse()
    {
        var (service, context) = CreateService();
        var userId = Guid.NewGuid();
        var monday = new DateOnly(2026, 5, 18);
        await SeedSubmissionAsync(context, userId, monday);

        var result = await service.UnsubmitWeekAsync(userId, monday);

        Assert.False(result.IsSubmitted);
    }
}
