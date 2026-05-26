using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Integration.Common;

namespace Api.Tests.Integration.TimeEntries;

public class TimeEntryMonthEndpointShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Return_Ok_For_Empty_Month()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Return_YearMonth_In_Response()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal("2026-06", json.GetProperty("yearMonth").GetString());
    }

    [Fact]
    public async Task Return_Correct_Visible_Window_For_June2026()
    {
        // June 2026: starts Monday June 1 → fromDate=2026-06-01, toDate=2026-07-05
        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal("2026-06-01", json.GetProperty("fromDate").GetString());
        Assert.Equal("2026-07-05", json.GetProperty("toDate").GetString());
    }

    [Fact]
    public async Task Return_Days_Covering_Every_Date_In_Window()
    {
        // June 2026 window: 2026-06-01 to 2026-07-05 = 35 days
        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        var days = json.GetProperty("days");
        Assert.Equal(35, days.GetArrayLength());
    }

    [Fact]
    public async Task Return_Days_With_Zero_Hours_And_Empty_Entries()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        foreach (var day in json.GetProperty("days").EnumerateArray())
        {
            Assert.Equal(0, day.GetProperty("totalHours").GetDecimal());
            Assert.Equal(0, day.GetProperty("entries").GetArrayLength());
        }
    }

    [Fact]
    public async Task Return_WeekSubmissions_Empty_In_This_Slice()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal(0, json.GetProperty("weekSubmissions").GetArrayLength());
    }

    [Fact]
    public async Task Return_BadRequest_For_Invalid_YearMonth()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/months/not-a-month");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
