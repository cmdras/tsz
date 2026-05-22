using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.TimeEntries;

public class TimeEntryPutEndpointShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static readonly DateOnly WeekStart = new(2026, 5, 18);

    [Fact]
    public async Task Return_Ok_With_Updated_Rows_After_Save()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(WeekStart);
        var body = new
        {
            cells = new[] { new { contractTaskId = taskId, leaveTypeId = (Guid?)null, date = "2026-05-18", hours = 8.0 } }
        };

        var response = await factory.Client.PutAsJsonAsync("/api/time-entries/weeks/2026-05-18", body);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.Equal(1, json.GetProperty("rows").GetArrayLength());
        Assert.Equal(taskId.ToString(), json.GetProperty("rows")[0].GetProperty("contractTaskId").GetString());
    }

    [Fact]
    public async Task Persist_Hours_So_Reload_Returns_Them()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(WeekStart);
        var body = new
        {
            cells = new[] { new { contractTaskId = taskId, leaveTypeId = (Guid?)null, date = "2026-05-18", hours = 4.5 } }
        };

        await factory.Client.PutAsJsonAsync("/api/time-entries/weeks/2026-05-18", body);

        var getResponse = await factory.Client.GetAsync("/api/time-entries/weeks/2026-05-18");
        getResponse.EnsureSuccessStatusCode();
        var json = await getResponse.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        var hoursArray = json.GetProperty("rows")[0].GetProperty("hours");
        Assert.Equal(4.5, hoursArray[0].GetDouble()); // Monday = index 0
    }

    [Fact]
    public async Task Update_LastSavedAt_After_Save()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(WeekStart);
        var body = new
        {
            cells = new[] { new { contractTaskId = taskId, leaveTypeId = (Guid?)null, date = "2026-05-18", hours = 8.0 } }
        };

        var response = await factory.Client.PutAsJsonAsync("/api/time-entries/weeks/2026-05-18", body);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.NotEqual(JsonValueKind.Null, json.GetProperty("lastSavedAt").ValueKind);
    }

    [Fact]
    public async Task Return_409_When_Week_Is_Already_Submitted()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Api.Common.Database.AppDbContext>();
        context.WeekSubmissions.Add(new Api.Modules.TimeEntries.WeekSubmission
        {
            Id = Guid.NewGuid(),
            UserId = Api.Tests.Integration.Common.TestAuthHandler.CurrentUserId,
            WeekStart = WeekStart,
            SubmittedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var response = await factory.Client.PutAsJsonAsync("/api/time-entries/weeks/2026-05-18", new { cells = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Off_Grain_Hours()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(WeekStart);
        var body = new
        {
            cells = new[] { new { contractTaskId = taskId, leaveTypeId = (Guid?)null, date = "2026-05-18", hours = 0.25 } }
        };

        var response = await factory.Client.PutAsJsonAsync("/api/time-entries/weeks/2026-05-18", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
