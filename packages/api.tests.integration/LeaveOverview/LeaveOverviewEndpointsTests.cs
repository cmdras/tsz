using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Modules.UserLeaveAllowances;
using Api.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.LeaveOverview;

public class LeaveOverviewEndpointsShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedAllowanceAsync(Guid userId, Guid leaveTypeId, int year, decimal totalDays, AllowanceMode mode = AllowanceMode.Limited)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Api.Common.Database.AppDbContext>();
        context.UserLeaveAllowances.Add(new UserLeaveAllowance
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeaveTypeId = leaveTypeId,
            Year = year,
            Mode = mode,
            TotalDays = totalDays,
        });
        await context.SaveChangesAsync();
    }

    // --- Happy path: seeded user + allowances + entries ---

    [Fact]
    public async Task Given_UserWithAllowancesAndEntries_When_GettingOverview_Then_ReturnsCorrectShape()
    {
        var leaveType = await factory.SeedLeaveTypeAsync("Annual Leave", 20m);
        await SeedAllowanceAsync(TestAuthHandler.CurrentUserId, leaveType.Id, 2026, 20m);
        await factory.SeedTimeEntryAsync(new DateOnly(2026, 3, 2), null, leaveType.Id, 8m);

        var response = await factory.Client.GetAsync("/api/leave-overview?year=2026");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal(2026, json.GetProperty("year").GetInt32());
        Assert.Equal(1, json.GetProperty("types").GetArrayLength());
        Assert.Equal(1, json.GetProperty("days").GetArrayLength());

        var typeItem = json.GetProperty("types")[0];
        Assert.Equal("Annual Leave", typeItem.GetProperty("name").GetString());
        Assert.Equal("Limited", typeItem.GetProperty("mode").GetString());
        Assert.Equal(20m, typeItem.GetProperty("allowance").GetDecimal());
        Assert.Equal(1m, typeItem.GetProperty("takenDays").GetDecimal());

        var dayItem = json.GetProperty("days")[0];
        Assert.Equal("2026-03-02", dayItem.GetProperty("date").GetString());
        var leaveTypeIds = dayItem.GetProperty("leaveTypeIds");
        Assert.Equal(1, leaveTypeIds.GetArrayLength());
        Assert.Equal(leaveType.Id.ToString(), leaveTypeIds[0].GetString());
    }

    // --- Year with no data returns empty days and zeroed types ---

    [Fact]
    public async Task Given_YearWithNoData_When_GettingOverview_Then_EmptyDaysAndZeroedTypes()
    {
        var leaveType = await factory.SeedLeaveTypeAsync("Annual Leave", 20m);
        await SeedAllowanceAsync(TestAuthHandler.CurrentUserId, leaveType.Id, 2026, 20m);

        var response = await factory.Client.GetAsync("/api/leave-overview?year=2025");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Empty(json.GetProperty("days").EnumerateArray());
        // types still returned (non-archived leave types always appear), but with zero takenDays
        var typeItem = json.GetProperty("types")[0];
        Assert.Equal(0m, typeItem.GetProperty("takenDays").GetDecimal());
    }

    // --- Entries for other users excluded ---

    [Fact]
    public async Task Given_OtherUsersEntries_When_GettingOverview_Then_OtherUsersEntriesExcluded()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Api.Common.Database.AppDbContext>();

        var otherUserId = Guid.NewGuid();
        context.Users.Add(new Api.Modules.Users.User
        {
            Id = otherUserId,
            Name = "Other User",
            Email = "other@test.com",
            Role = Api.Modules.Users.UserRole.User,
            IsArchived = false,
        });
        await context.SaveChangesAsync();

        var leaveType = await factory.SeedLeaveTypeAsync("Annual Leave", 20m);

        // Seed entry for the OTHER user
        context.TimeEntries.Add(new Api.Modules.TimeEntries.TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Date = new DateOnly(2026, 5, 4),
            LeaveTypeId = leaveType.Id,
            Hours = 8m,
            UpdatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var response = await factory.Client.GetAsync("/api/leave-overview?year=2026");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        // No days for current user (only other user had entries)
        Assert.Empty(json.GetProperty("days").EnumerateArray());
        // takenDays should be 0 for the current user
        var typeItem = json.GetProperty("types")[0];
        Assert.Equal(0m, typeItem.GetProperty("takenDays").GetDecimal());
    }

    // --- Invalid year returns 400 ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10000)]
    public async Task Given_InvalidYear_When_GettingOverview_Then_ReturnsBadRequest(int invalidYear)
    {
        var response = await factory.Client.GetAsync($"/api/leave-overview?year={invalidYear}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    // --- 401 unauthenticated is covered by AuthEnforcementShould ---
    // The AuthEnforcementShould test class asserts that every /api/* endpoint requires IAuthorizeData,
    // which guarantees that GET /api/leave-overview requires authentication.
}
