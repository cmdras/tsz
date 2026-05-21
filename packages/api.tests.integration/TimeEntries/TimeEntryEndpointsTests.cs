using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Integration.Common;

namespace Api.Tests.Integration.TimeEntries;

public class TimeEntryEndpointsShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Return_Ok_For_Empty_Week()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/weeks/2026-05-18");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Return_Week_With_Correct_WeekStart()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/weeks/2026-05-18");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal("2026-05-18", json.GetProperty("weekStart").GetString());
    }

    [Fact]
    public async Task Return_Week_Not_Submitted_By_Default()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/weeks/2026-05-18");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.False(json.GetProperty("isSubmitted").GetBoolean());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("submittedAt").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.GetProperty("lastSavedAt").ValueKind);
    }

    [Fact]
    public async Task Return_Week_With_Empty_Rows_And_Summary()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/weeks/2026-05-18");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal(0, json.GetProperty("rows").GetArrayLength());
        Assert.Equal(0, json.GetProperty("previousWeekSummary").GetProperty("chips").GetArrayLength());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("previousWeekSummary").GetProperty("overflow").ValueKind);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Invalid_Date()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/weeks/not-a-date");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
