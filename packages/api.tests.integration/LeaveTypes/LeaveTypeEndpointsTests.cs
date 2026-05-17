using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Database;
using Api.Modules.LeaveTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.LeaveTypes;

public class LeaveTypeEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public LeaveTypeEndpointsTests(WebApplicationFactory<Program> factory)
    {
        var guid = Guid.NewGuid();
        _testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var toRemove = services
                    .Where(descriptor =>
                        descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>) ||
                        descriptor.ServiceType == typeof(AppDbContext))
                    .ToList();
                foreach (var descriptor in toRemove)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options => options
                    .UseInMemoryDatabase("LeaveTypeIntegrationTests_" + guid)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            });
        });
        _client = _testFactory.CreateClient();
    }

    private async Task<LeaveType> SeedLeaveTypeViaApiAsync(string name = "Holiday", decimal defaultDays = 20m)
    {
        var request = new LeaveTypeRequest { Name = name, DefaultDays = defaultDays };
        var response = await _client.PostAsJsonAsync("/api/leave-types", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LeaveType>(JsonOptions))!;
    }

    [Fact]
    public async Task GetLeaveTypes_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/leave-types");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedLeaveTypes>(JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetLeaveTypeById_ExistingId_ReturnsOk()
    {
        var seeded = await SeedLeaveTypeViaApiAsync("ADV", 5m);

        var response = await _client.GetAsync($"/api/leave-types/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var leaveType = await response.Content.ReadFromJsonAsync<LeaveType>(JsonOptions);
        Assert.NotNull(leaveType);
        Assert.Equal("ADV", leaveType.Name);
    }

    [Fact]
    public async Task GetLeaveTypeById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/leave-types/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveType_ValidRequest_ReturnsCreated()
    {
        var request = new LeaveTypeRequest { Name = "Ancienniteit", DefaultDays = 0m };

        var response = await _client.PostAsJsonAsync("/api/leave-types", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var leaveType = await response.Content.ReadFromJsonAsync<LeaveType>(JsonOptions);
        Assert.NotNull(leaveType);
        Assert.Equal("Ancienniteit", leaveType.Name);
        Assert.Equal(0m, leaveType.DefaultDays);
    }

    [Fact]
    public async Task CreateLeaveType_DuplicateName_ReturnsConflict()
    {
        await SeedLeaveTypeViaApiAsync("Sickness", 0m);

        var response = await _client.PostAsJsonAsync("/api/leave-types", new LeaveTypeRequest { Name = "Sickness", DefaultDays = 5m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveType_DuplicateNameDifferentCase_ReturnsConflict()
    {
        await SeedLeaveTypeViaApiAsync("Holiday", 20m);

        var response = await _client.PostAsJsonAsync("/api/leave-types", new LeaveTypeRequest { Name = "HOLIDAY", DefaultDays = 20m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveType_InvalidRequest_ReturnsBadRequest()
    {
        var request = new { Name = "" };

        var response = await _client.PostAsJsonAsync("/api/leave-types", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveType_DefaultDaysWithTwoDecimalPlaces_ReturnsBadRequest()
    {
        var request = new LeaveTypeRequest { Name = "Bonus", DefaultDays = 7.25m };

        var response = await _client.PostAsJsonAsync("/api/leave-types", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLeaveType_ValidRequest_ReturnsOk()
    {
        var seeded = await SeedLeaveTypeViaApiAsync("Holiday Replacement", 0m);
        var request = new LeaveTypeRequest { Name = "Holiday Replacement", DefaultDays = 2.5m };

        var response = await _client.PutAsJsonAsync($"/api/leave-types/{seeded.Id}", request);

        response.EnsureSuccessStatusCode();
        var leaveType = await response.Content.ReadFromJsonAsync<LeaveType>(JsonOptions);
        Assert.NotNull(leaveType);
        Assert.Equal(2.5m, leaveType.DefaultDays);
    }

    [Fact]
    public async Task UpdateLeaveType_NonExistingId_ReturnsNotFound()
    {
        var request = new LeaveTypeRequest { Name = "Ghost", DefaultDays = 0m };

        var response = await _client.PutAsJsonAsync($"/api/leave-types/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLeaveType_DefaultDaysWithTwoDecimalPlaces_ReturnsBadRequest()
    {
        var seeded = await SeedLeaveTypeViaApiAsync("Bonus", 0m);
        var request = new LeaveTypeRequest { Name = "Bonus", DefaultDays = 7.25m };

        var response = await _client.PutAsJsonAsync($"/api/leave-types/{seeded.Id}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLeaveType_DuplicateName_ReturnsConflict()
    {
        await SeedLeaveTypeViaApiAsync("Holiday", 20m);
        var adv = await SeedLeaveTypeViaApiAsync("ADV", 5m);

        var response = await _client.PutAsJsonAsync($"/api/leave-types/{adv.Id}", new LeaveTypeRequest { Name = "Holiday", DefaultDays = 5m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveLeaveType_ExistingId_ReturnsNoContent()
    {
        var seeded = await SeedLeaveTypeViaApiAsync("Ancienniteit", 0m);

        var response = await _client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveLeaveType_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/leave-types/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnarchiveLeaveType_ExistingArchivedLeaveType_ReturnsNoContent()
    {
        var seeded = await SeedLeaveTypeViaApiAsync("ADV", 5m);
        await _client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        var response = await _client.PatchAsync($"/api/leave-types/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetLeaveTypes_ArchivedLeaveTypes_ExcludedByDefault()
    {
        var seeded = await SeedLeaveTypeViaApiAsync("To Be Archived", 0m);
        await _client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        var response = await _client.GetAsync("/api/leave-types?search=To+Be+Archived");
        var result = await response.Content.ReadFromJsonAsync<PagedLeaveTypes>(JsonOptions);

        Assert.Equal(0, result!.Total);
    }

    [Fact]
    public async Task GetLeaveTypes_ShowArchived_IncludesArchivedLeaveTypes()
    {
        var seeded = await SeedLeaveTypeViaApiAsync("Archived Type", 0m);
        await _client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        var response = await _client.GetAsync("/api/leave-types?showArchived=true&search=Archived+Type");
        var result = await response.Content.ReadFromJsonAsync<PagedLeaveTypes>(JsonOptions);

        Assert.Equal(1, result!.Total);
    }
}
