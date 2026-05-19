using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Database;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Api.Tests.Integration.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Users;

public class UserLeaveAllowanceEndpointsTests : IClassFixture<UserLeaveAllowanceApiFactory>, IAsyncLifetime
{
    private readonly UserLeaveAllowanceApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public UserLeaveAllowanceEndpointsTests(UserLeaveAllowanceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.UserLeaveAllowances.RemoveRange(context.UserLeaveAllowances);
        context.Users.RemoveRange(context.Users);
        context.LeaveTypes.RemoveRange(context.LeaveTypes);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private AppDbContext OpenContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_factory.DatabaseName)
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private async Task<LeaveType> SeedLeaveTypeAsync(string name, decimal defaultDays, AllowanceMode defaultMode)
    {
        await using var context = OpenContext();
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = name,
            DefaultDays = defaultDays,
            DefaultMode = defaultMode,
        };
        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync();
        return leaveType;
    }

    private async Task<JsonElement> CreateUserAsync(string name = "Alice", string email = "alice@test.com", UserRole role = UserRole.User)
    {
        var request = new UserRequest { Name = name, Email = email, Role = role };
        var response = await _client.PostAsJsonAsync("/api/users", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions))!;
    }

    [Fact]
    public async Task GetUserById_IncludesLeavesArrayInResponse()
    {
        await SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync();
        var id = created.GetProperty("id").GetGuid();

        var response = await _client.GetAsync($"/api/users/{id}");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(json.TryGetProperty("leaves", out var leaves), "Response did not include 'leaves' property.");
        Assert.Equal(JsonValueKind.Array, leaves.ValueKind);
    }

    [Fact]
    public async Task CreateUser_AutoPopulatesLeavesFromActiveLeaveTypes()
    {
        var holiday = await SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var sickness = await SeedLeaveTypeAsync("Sickness", 0m, AllowanceMode.Unlimited);

        var created = await CreateUserAsync("Bob", "bob@test.com");
        var id = created.GetProperty("id").GetGuid();

        await using var context = OpenContext();
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
    public async Task UpdateUser_WithLeaves_PersistsChanges()
    {
        var holiday = await SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync("Carol", "carol@test.com");
        var id = created.GetProperty("id").GetGuid();

        Guid existingAllowanceId;
        await using (var context = OpenContext())
        {
            var existing = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == id);
            existingAllowanceId = existing.Id;
        }

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

        var response = await _client.PutAsJsonAsync($"/api/users/{id}", updateRequest);

        response.EnsureSuccessStatusCode();
        await using var verification = OpenContext();
        var refreshed = await verification.UserLeaveAllowances
            .AsNoTracking()
            .FirstAsync(allowance => allowance.Id == existingAllowanceId);
        Assert.Equal(25m, refreshed.TotalDays);
    }

    [Fact]
    public async Task UpdateUser_RemoveLeave_HardDeletesAllowance()
    {
        await SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync("Dave", "dave@test.com");
        var id = created.GetProperty("id").GetGuid();

        Guid existingAllowanceId;
        await using (var context = OpenContext())
        {
            var existing = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == id);
            existingAllowanceId = existing.Id;
        }

        var updateRequest = new UserRequest
        {
            Name = "Dave",
            Email = "dave@test.com",
            Role = UserRole.User,
            Leaves = [],
        };

        var response = await _client.PutAsJsonAsync($"/api/users/{id}", updateRequest);

        response.EnsureSuccessStatusCode();
        await using var verification = OpenContext();
        var stillExists = await verification.UserLeaveAllowances
            .AsNoTracking()
            .AnyAsync(allowance => allowance.Id == existingAllowanceId);
        Assert.False(stillExists);
    }

    [Fact]
    public async Task UpdateUser_DuplicateLeaveTypeInSameYear_ReturnsConflict()
    {
        var holiday = await SeedLeaveTypeAsync("Holiday", 20m, AllowanceMode.Limited);
        var created = await CreateUserAsync("Eve", "eve@test.com");
        var id = created.GetProperty("id").GetGuid();

        Guid existingAllowanceId;
        await using (var context = OpenContext())
        {
            var existing = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == id);
            existingAllowanceId = existing.Id;
        }

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

        var response = await _client.PutAsJsonAsync($"/api/users/{id}", updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveType_WithDefaultModeLimited_RoundTripsDefaultMode()
    {
        var request = new LeaveTypeRequest
        {
            Name = "BonusHoliday",
            DefaultDays = 5m,
            DefaultMode = AllowanceMode.Limited,
        };

        var response = await _client.PostAsJsonAsync("/api/leave-types", request);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(json.TryGetProperty("defaultMode", out var defaultMode), "Response did not include 'defaultMode' property.");
        Assert.Equal("Limited", defaultMode.GetString());
    }
}

public class UserLeaveAllowanceApiFactory : TestApiFactory
{
    public UserLeaveAllowanceApiFactory() : base("UserLeaveAllowanceIntegrationTests") { }
}
