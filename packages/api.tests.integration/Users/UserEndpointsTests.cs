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

public class UserEndpointsShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<User> SeedUserAsync(string name = "Alice", string email = "alice@test.com", UserRole role = UserRole.User)
    {
        var request = new UserRequest { Name = name, Email = email, Role = role };
        var response = await factory.Client.PostAsJsonAsync("/api/users", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<User>(IntegrationFactory.JsonOptions))!;
    }

    [Fact]
    public async Task Return_Ok_For_User_List()
    {
        var response = await factory.Client.GetAsync("/api/users");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedUsers>(IntegrationFactory.JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Find_User_By_Id()
    {
        var seeded = await SeedUserAsync("Bob", "bob@test.com");

        var response = await factory.Client.GetAsync($"/api/users/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<User>(IntegrationFactory.JsonOptions);
        Assert.NotNull(user);
        Assert.Equal("Bob", user.Name);
    }

    [Fact]
    public async Task Return_Not_Found_For_Unknown_User()
    {
        var response = await factory.Client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_User_And_Return_Created()
    {
        var request = new UserRequest { Name = "Carol", Email = "carol@test.com", Role = UserRole.Admin };

        var response = await factory.Client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<User>(IntegrationFactory.JsonOptions);
        Assert.NotNull(user);
        Assert.Equal("Carol", user.Name);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Invalid_User()
    {
        var request = new { Name = "", Email = "not-an-email" };

        var response = await factory.Client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Duplicate_Email_On_Create()
    {
        await SeedUserAsync("Dave", "dave@test.com");
        var duplicate = new UserRequest { Name = "Dave2", Email = "dave@test.com", Role = UserRole.User };

        var response = await factory.Client.PostAsJsonAsync("/api/users", duplicate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Duplicate_Email_Different_Case_On_Create()
    {
        await SeedUserAsync("Eve", "eve@test.com");
        var duplicate = new UserRequest { Name = "Eve2", Email = "EVE@TEST.COM", Role = UserRole.User };

        var response = await factory.Client.PostAsJsonAsync("/api/users", duplicate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Return_Conflict_Error_For_Duplicate_Email()
    {
        await SeedUserAsync("Frank", "frank@test.com");
        var duplicate = new UserRequest { Name = "Frank2", Email = "frank@test.com", Role = UserRole.User };

        var response = await factory.Client.PostAsJsonAsync("/api/users", duplicate);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.Conflict, body.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task Update_User_Fields()
    {
        var seeded = await SeedUserAsync("Eve", "eve@test.com");
        var request = new UserRequest { Name = "Eve Updated", Email = "eve-updated@test.com", Role = UserRole.Admin };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{seeded.Id}", request);

        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<User>(IntegrationFactory.JsonOptions);
        Assert.Equal("Eve Updated", user!.Name);
    }

    [Fact]
    public async Task Reject_Duplicate_Email_On_Update()
    {
        var first = await SeedUserAsync("Frank", "frank@test.com");
        await SeedUserAsync("Grace", "grace@test.com");
        var request = new UserRequest { Name = "Frank", Email = "grace@test.com", Role = UserRole.User };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{first.Id}", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Duplicate_Email_Different_Case_On_Update()
    {
        var first = await SeedUserAsync("Hank", "hank@test.com");
        await SeedUserAsync("Irene", "irene@test.com");
        var request = new UserRequest { Name = "Hank", Email = "IRENE@TEST.COM", Role = UserRole.User };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{first.Id}", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Unknown_LeaveType_On_Update()
    {
        var seeded = await SeedUserAsync("Jack", "jack@test.com");
        var request = new UserRequest
        {
            Name = "Jack",
            Email = "jack@test.com",
            Role = UserRole.User,
            Leaves =
            [
                new UserLeaveAllowanceRequest
                {
                    Id = null,
                    LeaveTypeId = Guid.NewGuid(),
                    Mode = AllowanceMode.Limited,
                    TotalDays = 20m,
                },
            ],
        };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{seeded.Id}", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Persist_New_Allowance_When_LeaveType_Not_Yet_On_User()
    {
        using var setupScope = factory.Services.CreateScope();
        var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(), Name = "Holiday", DefaultDays = 20m, DefaultMode = AllowanceMode.Limited,
        };
        var newLeaveType = new LeaveType
        {
            Id = Guid.NewGuid(), Name = "ADV", DefaultDays = 5m, DefaultMode = AllowanceMode.Limited,
        };
        setupContext.LeaveTypes.AddRange(leaveType, newLeaveType);
        await setupContext.SaveChangesAsync();

        var seeded = await SeedUserAsync("Kate", "kate@test.com");

        Guid existingAllowanceId;
        using (var scope3 = factory.Services.CreateScope())
        {
            var context3 = scope3.ServiceProvider.GetRequiredService<AppDbContext>();
            var existing = await context3.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == seeded.Id && allowance.LeaveTypeId == leaveType.Id);
            existingAllowanceId = existing.Id;
        }

        var request = new UserRequest
        {
            Name = "Kate",
            Email = "kate@test.com",
            Role = UserRole.User,
            Leaves =
            [
                new UserLeaveAllowanceRequest
                {
                    Id = existingAllowanceId,
                    LeaveTypeId = leaveType.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 20m,
                },
                new UserLeaveAllowanceRequest
                {
                    Id = null,
                    LeaveTypeId = newLeaveType.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 5m,
                },
            ],
        };

        var response = await factory.Client.PutAsJsonAsync($"/api/users/{seeded.Id}", request);

        response.EnsureSuccessStatusCode();
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var allowances = await verifyContext.UserLeaveAllowances
            .Where(allowance => allowance.UserId == seeded.Id)
            .ToListAsync();
        Assert.Equal(2, allowances.Count);
        Assert.Contains(allowances, allowance => allowance.LeaveTypeId == newLeaveType.Id && allowance.TotalDays == 5m);
    }

    [Fact]
    public async Task Archive_User()
    {
        var seeded = await SeedUserAsync("Henry", "henry@test.com");

        var response = await factory.Client.PatchAsync($"/api/users/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Return_Not_Found_On_Archive_With_Unknown_Id()
    {
        var response = await factory.Client.PatchAsync($"/api/users/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unarchive_User()
    {
        var seeded = await SeedUserAsync("Irene", "irene@test.com");
        await factory.Client.PatchAsync($"/api/users/{seeded.Id}/archive", null);

        var response = await factory.Client.PatchAsync($"/api/users/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Exclude_Archived_Users_From_List()
    {
        var seeded = await SeedUserAsync("Zephyr", "zephyr@test.com");
        await factory.Client.PatchAsync($"/api/users/{seeded.Id}/archive", null);

        var response = await factory.Client.GetAsync("/api/users?search=zephyr");
        var result = await response.Content.ReadFromJsonAsync<PagedUsers>(IntegrationFactory.JsonOptions);

        Assert.Equal(0, result!.Total);
    }

    [Fact]
    public async Task RoundTrip_Sort_And_Pagination_Query_String()
    {
        for (var index = 1; index <= 4; index++)
            await SeedUserAsync($"User {index:D2}", $"user{index:D2}@test.com");

        var page1 = await factory.Client.GetFromJsonAsync<PagedUsers>(
            "/api/users?sort=Name&sortDirection=Asc&page=1&pageSize=2",
            IntegrationFactory.JsonOptions);
        var page2 = await factory.Client.GetFromJsonAsync<PagedUsers>(
            "/api/users?sort=Name&sortDirection=Asc&page=2&pageSize=2",
            IntegrationFactory.JsonOptions);

        Assert.Equal(4, page1!.Total);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(2, page2!.Items.Count);
        Assert.True(string.Compare(page1.Items[0].Name, page1.Items[1].Name, StringComparison.Ordinal) < 0);
        Assert.True(string.Compare(page1.Items.Last().Name, page2.Items[0].Name, StringComparison.Ordinal) < 0);
    }
}
