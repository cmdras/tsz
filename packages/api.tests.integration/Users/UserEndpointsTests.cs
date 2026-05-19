using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Database;
using Api.Modules.Users;
using Api.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Users;

public class UserEndpointsTests : IClassFixture<UserApiFactory>, IAsyncLifetime
{
    private readonly UserApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public UserEndpointsTests(UserApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<User> SeedUserViaApiAsync(string name = "Alice", string email = "alice@test.com", UserRole role = UserRole.User)
    {
        var request = new UserRequest { Name = name, Email = email, Role = role };
        var response = await _client.PostAsJsonAsync("/api/users", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<User>(JsonOptions))!;
    }

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/users");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedUsers>(JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserById_ExistingId_ReturnsOk()
    {
        var seeded = await SeedUserViaApiAsync("Bob", "bob@test.com");

        var response = await _client.GetAsync($"/api/users/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(user);
        Assert.Equal("Bob", user.Name);
    }

    [Fact]
    public async Task GetUserById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        var request = new UserRequest { Name = "Carol", Email = "carol@test.com", Role = UserRole.Admin };

        var response = await _client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(user);
        Assert.Equal("Carol", user.Name);
    }

    [Fact]
    public async Task CreateUser_InvalidRequest_ReturnsBadRequest()
    {
        var request = new { Name = "", Email = "not-an-email" };

        var response = await _client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsConflict()
    {
        await SeedUserViaApiAsync("Dave", "dave@test.com");
        var duplicate = new UserRequest { Name = "Dave2", Email = "dave@test.com", Role = UserRole.User };

        var response = await _client.PostAsJsonAsync("/api/users", duplicate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ValidRequest_ReturnsOk()
    {
        var seeded = await SeedUserViaApiAsync("Eve", "eve@test.com");
        var request = new UserRequest { Name = "Eve Updated", Email = "eve-updated@test.com", Role = UserRole.Admin };

        var response = await _client.PutAsJsonAsync($"/api/users/{seeded.Id}", request);

        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.Equal("Eve Updated", user!.Name);
    }

    [Fact]
    public async Task UpdateUser_DuplicateEmail_ReturnsConflict()
    {
        var first = await SeedUserViaApiAsync("Frank", "frank@test.com");
        await SeedUserViaApiAsync("Grace", "grace@test.com");
        var request = new UserRequest { Name = "Frank", Email = "grace@test.com", Role = UserRole.User };

        var response = await _client.PutAsJsonAsync($"/api/users/{first.Id}", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveUser_ExistingId_ReturnsNoContent()
    {
        var seeded = await SeedUserViaApiAsync("Henry", "henry@test.com");

        var response = await _client.PatchAsync($"/api/users/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveUser_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/users/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnarchiveUser_ExistingArchivedUser_ReturnsNoContent()
    {
        var seeded = await SeedUserViaApiAsync("Irene", "irene@test.com");
        await _client.PatchAsync($"/api/users/{seeded.Id}/archive", null);

        var response = await _client.PatchAsync($"/api/users/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ArchivedUsers_ExcludedFromList()
    {
        var seeded = await SeedUserViaApiAsync("Zephyr", "zephyr@test.com");
        await _client.PatchAsync($"/api/users/{seeded.Id}/archive", null);

        var response = await _client.GetAsync("/api/users?search=zephyr");
        var result = await response.Content.ReadFromJsonAsync<PagedUsers>(JsonOptions);

        Assert.Equal(0, result!.Total);
    }
}

public class UserApiFactory : TestApiFactory
{
    public UserApiFactory() : base("UserIntegrationTests") { }
}
