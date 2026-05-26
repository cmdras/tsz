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

    // --- Entries in response ---

    [Fact]
    public async Task Given_TaskEntry_When_FetchingMonth_Then_DayHasEntryWithKindAndDenormalizedNames()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(new DateOnly(2026, 6, 1));
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 6, 1), taskId, null, 8m);

        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        var june1 = json.GetProperty("days").EnumerateArray()
            .Single(day => day.GetProperty("date").GetString() == "2026-06-01");
        Assert.Equal(8m, june1.GetProperty("totalHours").GetDecimal());
        var entries = june1.GetProperty("entries");
        Assert.Equal(1, entries.GetArrayLength());
        var entry = entries[0];
        Assert.Equal("task", entry.GetProperty("kind").GetString());
        Assert.Equal(8m, entry.GetProperty("hours").GetDecimal());
        Assert.NotNull(entry.GetProperty("customerName").GetString());
        Assert.NotNull(entry.GetProperty("contractSubject").GetString());
        Assert.NotNull(entry.GetProperty("taskName").GetString());
    }

    [Fact]
    public async Task Given_LeaveEntry_When_FetchingMonth_Then_DayHasEntryWithLeaveKind()
    {
        var leaveType = await factory.SeedLeaveTypeAsync("Annual Leave");
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 6, 2), null, leaveType.Id, 8m);

        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        var june2 = json.GetProperty("days").EnumerateArray()
            .Single(day => day.GetProperty("date").GetString() == "2026-06-02");
        Assert.Equal(8m, june2.GetProperty("totalHours").GetDecimal());
        var entries = june2.GetProperty("entries");
        Assert.Equal(1, entries.GetArrayLength());
        var entry = entries[0];
        Assert.Equal("leave", entry.GetProperty("kind").GetString());
        Assert.Equal("Annual Leave", entry.GetProperty("leaveTypeName").GetString());
    }

    [Fact]
    public async Task Given_MixedWeek_When_FetchingMonth_Then_BothKindsAppearedWithCorrectTotals()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(new DateOnly(2026, 6, 1));
        var leaveType = await factory.SeedLeaveTypeAsync("Sick Leave");
        // Monday 2026-06-01: 8h task
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 6, 1), taskId, null, 8m);
        // Tuesday 2026-06-02: 4h task + 4h leave
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 6, 2), taskId, null, 4m);
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 6, 2), null, leaveType.Id, 4m);

        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        var june1 = json.GetProperty("days").EnumerateArray()
            .Single(day => day.GetProperty("date").GetString() == "2026-06-01");
        Assert.Equal(8m, june1.GetProperty("totalHours").GetDecimal());
        Assert.Equal(1, june1.GetProperty("entries").GetArrayLength());

        var june2 = json.GetProperty("days").EnumerateArray()
            .Single(day => day.GetProperty("date").GetString() == "2026-06-02");
        Assert.Equal(8m, june2.GetProperty("totalHours").GetDecimal());
        Assert.Equal(2, june2.GetProperty("entries").GetArrayLength());
    }

    [Fact]
    public async Task Given_WeekendEntry_When_FetchingMonth_Then_WeekendDayHasEntryAndCountsInTotal()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(new DateOnly(2026, 6, 1));
        // Saturday 2026-06-06
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 6, 6), taskId, null, 4m);

        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        var june6 = json.GetProperty("days").EnumerateArray()
            .Single(day => day.GetProperty("date").GetString() == "2026-06-06");
        Assert.Equal(4m, june6.GetProperty("totalHours").GetDecimal());
        Assert.Equal(1, june6.GetProperty("entries").GetArrayLength());
    }

    [Fact]
    public async Task Given_OutOfMonthEntry_When_FetchingMonth_Then_DayHasEntryWithIsInMonthFalse()
    {
        var (_, taskId) = await factory.SeedUserWithContractTaskAsync(new DateOnly(2026, 6, 1));
        // July 5 is in the visible window for June 2026 but out-of-month
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 7, 5), taskId, null, 7m);

        var response = await factory.Client.GetAsync("/api/time-entries/months/2026-06");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        var july5 = json.GetProperty("days").EnumerateArray()
            .Single(day => day.GetProperty("date").GetString() == "2026-07-05");
        Assert.False(july5.GetProperty("isInMonth").GetBoolean());
        Assert.Equal(7m, july5.GetProperty("totalHours").GetDecimal());
        Assert.Equal(1, july5.GetProperty("entries").GetArrayLength());
    }
}
