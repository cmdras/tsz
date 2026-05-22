using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.TimeEntries;

public class TimeEntrySubmitEndpointShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static readonly DateOnly WeekStart = new(2026, 5, 18);

    [Fact]
    public async Task Return_Ok_And_Mark_Submitted_On_Empty_Week()
    {
        var response = await factory.Client.PostAsJsonAsync(
            "/api/time-entries/weeks/2026-05-18/submit",
            new { cells = Array.Empty<object>() });

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.True(json.GetProperty("isSubmitted").GetBoolean());
        Assert.NotEqual(JsonValueKind.Null, json.GetProperty("submittedAt").ValueKind);
    }

    [Fact]
    public async Task Persist_Cells_And_Mark_Submitted_Atomically()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(WeekStart);
        var body = new
        {
            cells = new[] { new { contractTaskId = taskId, leaveTypeId = (Guid?)null, date = "2026-05-19", hours = 6.0 } }
        };

        var response = await factory.Client.PostAsJsonAsync("/api/time-entries/weeks/2026-05-18/submit", body);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.True(json.GetProperty("isSubmitted").GetBoolean());
        Assert.Equal(1, json.GetProperty("rows").GetArrayLength());
    }

    [Fact]
    public async Task Return_409_On_Second_Submit()
    {
        await factory.Client.PostAsJsonAsync(
            "/api/time-entries/weeks/2026-05-18/submit",
            new { cells = Array.Empty<object>() });

        var secondResponse = await factory.Client.PostAsJsonAsync(
            "/api/time-entries/weeks/2026-05-18/submit",
            new { cells = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Return_409_When_Week_Already_Saved_Via_Put_Then_Submitted_Twice()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Api.Common.Database.AppDbContext>();
        context.WeekSubmissions.Add(new Api.Modules.TimeEntries.WeekSubmission
        {
            Id = Guid.NewGuid(),
            UserId = TestAuthHandler.CurrentUserId,
            WeekStart = WeekStart,
            SubmittedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var response = await factory.Client.PostAsJsonAsync(
            "/api/time-entries/weeks/2026-05-18/submit",
            new { cells = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Non_Monday_WeekStart()
    {
        var response = await factory.Client.PostAsJsonAsync(
            "/api/time-entries/weeks/2026-05-19/submit",
            new { cells = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Prevent_Put_After_Submit()
    {
        await factory.Client.PostAsJsonAsync(
            "/api/time-entries/weeks/2026-05-18/submit",
            new { cells = Array.Empty<object>() });

        var putResponse = await factory.Client.PutAsJsonAsync(
            "/api/time-entries/weeks/2026-05-18",
            new { cells = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
    }
}
