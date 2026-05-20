using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Common.Database;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Api.Tests.Integration.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Users;

public class UserLeaveAllowanceEndpointsShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<JsonElement> CreateUserAsync(string name = "Alice", string email = "alice@test.com", UserRole role = UserRole.User)
    {
        var request = new UserRequest { Name = name, Email = email, Role = role };
        var response = await factory.Client.PostAsJsonAsync("/api/users", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions))!;
    }

    private async Task<Guid> GetFirstAllowanceIdAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var allowance = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == userId);
        return allowance.Id;
    }

    [Fact]
    public async Task Include_Leaves_Array_In_User_Response()
    {
        await factory.SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync();
        var id = created.GetProperty("id").GetGuid();

        var response = await factory.Client.GetAsync($"/api/users/{id}");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.True(json.TryGetProperty("leaves", out var leaves), "Response did not include 'leaves' property.");
        Assert.Equal(JsonValueKind.Array, leaves.ValueKind);
    }

    [Fact]
    public async Task Populate_Leaves_From_Active_Leave_Types_On_Create()
    {
        var holiday = await factory.SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var sickness = await factory.SeedLeaveTypeAsync("Sickness", 0m, AllowanceMode.Unlimited);

        var created = await CreateUserAsync("Bob", "bob@test.com");
        var id = created.GetProperty("id").GetGuid();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var allowances = await context.UserLeaveAllowances
            .Where(allowance => allowance.UserId == id)
            .ToListAsync();
        Assert.Equal(2, allowances.Count);
        Assert.Contains(allowances, allowance =>
            allowance.LeaveTypeId == holiday.Id &&
            allowance.Mode == AllowanceMode.Limited &&
            allowance.TotalDays == 20m);
        Assert.Contains(allowances, allowance =>
            allowance.LeaveTypeId == sickness.Id &&
            allowance.Mode == AllowanceMode.Unlimited);
    }

    [Fact]
    public async Task Persist_Updated_Leave_Allowances()
    {
        var holiday = await factory.SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync("Carol", "carol@test.com");
        var id = created.GetProperty("id").GetGuid();
        var existingAllowanceId = await GetFirstAllowanceIdAsync(id);

        var updateRequest = new UserRequest
        {
            Name = "Carol",
            Email = "carol@test.com",
            Role = UserRole.User,
            Leaves =
            [
                new UserLeaveAllowanceRequest
                {
                    Id = existingAllowanceId,
                    LeaveTypeId = holiday.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 25m,
                },
            ],
        };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{id}", updateRequest);

        response.EnsureSuccessStatusCode();
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshed = await verifyContext.UserLeaveAllowances
            .AsNoTracking()
            .FirstAsync(allowance => allowance.Id == existingAllowanceId);
        Assert.Equal(25m, refreshed.TotalDays);
    }

    [Fact]
    public async Task Delete_Leave_Allowance_On_Removal()
    {
        await factory.SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync("Dave", "dave@test.com");
        var id = created.GetProperty("id").GetGuid();
        var existingAllowanceId = await GetFirstAllowanceIdAsync(id);

        var updateRequest = new UserRequest
        {
            Name = "Dave",
            Email = "dave@test.com",
            Role = UserRole.User,
            Leaves = [],
        };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{id}", updateRequest);

        response.EnsureSuccessStatusCode();
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stillExists = await verifyContext.UserLeaveAllowances
            .AsNoTracking()
            .AnyAsync(allowance => allowance.Id == existingAllowanceId);
        Assert.False(stillExists);
    }

    [Fact]
    public async Task Reject_Duplicate_Leave_Type_In_Same_Year()
    {
        var holiday = await factory.SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync("Eve", "eve@test.com");
        var id = created.GetProperty("id").GetGuid();
        var existingAllowanceId = await GetFirstAllowanceIdAsync(id);

        var updateRequest = new UserRequest
        {
            Name = "Eve",
            Email = "eve@test.com",
            Role = UserRole.User,
            Leaves =
            [
                new UserLeaveAllowanceRequest
                {
                    Id = existingAllowanceId,
                    LeaveTypeId = holiday.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 20m,
                },
                new UserLeaveAllowanceRequest
                {
                    Id = null,
                    LeaveTypeId = holiday.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 5m,
                },
            ],
        };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{id}", updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Round_Trip_Default_Mode_Limited_For_New_Leave_Type()
    {
        var request = new LeaveTypeRequest
        {
            Name = "BonusHoliday",
            DefaultDays = 5m,
            DefaultMode = AllowanceMode.Limited,
        };

        var response = await factory.Client.PostAsJsonAsync("/api/leave-types", request);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.True(json.TryGetProperty("defaultMode", out var defaultMode), "Response did not include 'defaultMode' property.");
        Assert.Equal("Limited", defaultMode.GetString());
    }
}
