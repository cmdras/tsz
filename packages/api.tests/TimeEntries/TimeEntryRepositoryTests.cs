using Api.Common.Database;
using Api.Modules.TimeEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.TimeEntries;

public class TimeEntryRepositoryShould
{
    private static TimeEntryRepository CreateRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new TimeEntryRepository(context);
    }

    private static async Task SeedWeekSubmissionAsync(AppDbContext context, Guid userId, DateOnly weekStart, DateTime submittedAt)
    {
        context.WeekSubmissions.Add(new WeekSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WeekStart = weekStart,
            SubmittedAt = submittedAt,
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedTimeEntryAsync(AppDbContext context, Guid userId, DateOnly date, DateTime updatedAt)
    {
        context.TimeEntries.Add(new TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = date,
            ContractTaskId = Guid.NewGuid(),
            Hours = 8m,
            UpdatedAt = updatedAt,
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Return_Not_Submitted_When_No_Submission_Exists()
    {
        var repository = CreateRepository(out _);
        var userId = Guid.NewGuid();
        var weekStart = new DateOnly(2026, 5, 18);

        var weekData = await repository.GetWeekAsync(userId, weekStart);

        Assert.False(weekData.IsSubmitted);
        Assert.Null(weekData.SubmittedAt);
    }

    [Fact]
    public async Task Return_Submitted_When_Submission_Exists()
    {
        var repository = CreateRepository(out var context);
        var userId = Guid.NewGuid();
        var weekStart = new DateOnly(2026, 5, 18);
        var submittedAt = new DateTime(2026, 5, 22, 17, 0, 0, DateTimeKind.Utc);
        await SeedWeekSubmissionAsync(context, userId, weekStart, submittedAt);

        var weekData = await repository.GetWeekAsync(userId, weekStart);

        Assert.True(weekData.IsSubmitted);
        Assert.Equal(submittedAt, weekData.SubmittedAt);
    }

    [Fact]
    public async Task Return_Null_LastSavedAt_When_No_Entries_In_Week()
    {
        var repository = CreateRepository(out _);
        var userId = Guid.NewGuid();
        var weekStart = new DateOnly(2026, 5, 18);

        var weekData = await repository.GetWeekAsync(userId, weekStart);

        Assert.Null(weekData.LastSavedAt);
    }

    [Fact]
    public async Task Return_LastSavedAt_As_Max_UpdatedAt_In_Week()
    {
        var repository = CreateRepository(out var context);
        var userId = Guid.NewGuid();
        var weekStart = new DateOnly(2026, 5, 18);
        var earlier = new DateTime(2026, 5, 19, 9, 0, 0, DateTimeKind.Utc);
        var later = new DateTime(2026, 5, 20, 15, 30, 0, DateTimeKind.Utc);
        await SeedTimeEntryAsync(context, userId, new DateOnly(2026, 5, 19), earlier);
        await SeedTimeEntryAsync(context, userId, new DateOnly(2026, 5, 20), later);

        var weekData = await repository.GetWeekAsync(userId, weekStart);

        Assert.Equal(later, weekData.LastSavedAt);
    }

    [Fact]
    public async Task Exclude_Entries_Outside_The_Week()
    {
        var repository = CreateRepository(out var context);
        var userId = Guid.NewGuid();
        var weekStart = new DateOnly(2026, 5, 18);
        var inWeek = new DateTime(2026, 5, 19, 10, 0, 0, DateTimeKind.Utc);
        var nextWeek = new DateTime(2026, 5, 26, 10, 0, 0, DateTimeKind.Utc);
        await SeedTimeEntryAsync(context, userId, new DateOnly(2026, 5, 19), inWeek);
        await SeedTimeEntryAsync(context, userId, new DateOnly(2026, 5, 25), nextWeek);

        var weekData = await repository.GetWeekAsync(userId, weekStart);

        Assert.Equal(inWeek, weekData.LastSavedAt);
    }

    [Fact]
    public async Task Ignore_Submission_And_Entries_For_Other_User()
    {
        var repository = CreateRepository(out var context);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var weekStart = new DateOnly(2026, 5, 18);
        await SeedWeekSubmissionAsync(context, otherUserId, weekStart, DateTime.UtcNow);
        await SeedTimeEntryAsync(context, otherUserId, new DateOnly(2026, 5, 19), DateTime.UtcNow);

        var weekData = await repository.GetWeekAsync(userId, weekStart);

        Assert.False(weekData.IsSubmitted);
        Assert.Null(weekData.LastSavedAt);
    }
}
