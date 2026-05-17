using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Counters;
using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Contracts;

public class ContractEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly HttpClient _client;
    private Guid _customerId;
    private Guid _consultantId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public ContractEndpointsTests(WebApplicationFactory<Program> factory)
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
                    .UseInMemoryDatabase("ContractIntegrationTests_" + guid)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            });
        });
        _client = _testFactory.CreateClient();

        using var scope = _testFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Counters.Add(new Counter { Key = CounterKeys.Contract, Value = 99999 });

        var customer = new Api.Modules.Customers.Customer
        {
            Id = Guid.NewGuid(), Number = 100000, Name = "Test Customer", Country = "Belgium",
        };
        dbContext.Customers.Add(customer);
        _customerId = customer.Id;

        var consultant = new User
        {
            Id = Guid.NewGuid(), Name = "Test Consultant", Email = "consultant@test.com", Role = UserRole.User,
        };
        dbContext.Users.Add(consultant);
        _consultantId = consultant.Id;

        dbContext.SaveChanges();
    }

    private ContractRequest BuildRequest(string subject = "Test Contract") => new()
    {
        CustomerId = _customerId,
        ConsultantId = _consultantId,
        Subject = subject,
        StartDate = new DateOnly(2026, 1, 1),
        EndDate = new DateOnly(2026, 12, 31),
        Tasks = [new ContractTaskRequest { Name = "Analysis", DayRate = 800m }],
    };

    private async Task<Contract> SeedContractViaApiAsync(string subject = "Test Contract")
    {
        var response = await _client.PostAsJsonAsync("/api/contracts", BuildRequest(subject));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Contract>(JsonOptions))!;
    }

    [Fact]
    public async Task GetContracts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/contracts");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedContracts>(JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetContractById_ExistingId_ReturnsOk()
    {
        var seeded = await SeedContractViaApiAsync("Cloud Migration");

        var response = await _client.GetAsync($"/api/contracts/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var contract = await response.Content.ReadFromJsonAsync<Contract>(JsonOptions);
        Assert.NotNull(contract);
        Assert.Equal("Cloud Migration", contract.Subject);
    }

    [Fact]
    public async Task GetContractById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/contracts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateContract_ValidRequest_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/contracts", BuildRequest("New Contract"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var contract = await response.Content.ReadFromJsonAsync<Contract>(JsonOptions);
        Assert.NotNull(contract);
        Assert.Equal("New Contract", contract.Subject);
        Assert.True(contract.Number >= 100000);
    }

    [Fact]
    public async Task CreateContract_AssignsSequentialNumbers()
    {
        var first = await SeedContractViaApiAsync("First");
        var second = await SeedContractViaApiAsync("Second");

        Assert.Equal(first.Number + 1, second.Number);
    }

    [Fact]
    public async Task CreateContract_NoTasks_Returns422()
    {
        var request = BuildRequest();
        request.Tasks = [];

        var response = await _client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateContract_EndDateBeforeStartDate_Returns422()
    {
        var request = BuildRequest();
        request.StartDate = new DateOnly(2026, 6, 1);
        request.EndDate = new DateOnly(2026, 1, 1);

        var response = await _client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContract_ValidRequest_ReturnsOk()
    {
        var seeded = await SeedContractViaApiAsync("Original");
        var updateRequest = BuildRequest("Updated");
        updateRequest.Tasks = [new ContractTaskRequest { Id = seeded.Tasks.First().Id, Name = "Updated Task", DayRate = 950m }];

        var response = await _client.PutAsJsonAsync($"/api/contracts/{seeded.Id}", updateRequest);

        response.EnsureSuccessStatusCode();
        var contract = await response.Content.ReadFromJsonAsync<Contract>(JsonOptions);
        Assert.Equal("Updated", contract!.Subject);
    }

    [Fact]
    public async Task UpdateContract_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync($"/api/contracts/{Guid.NewGuid()}", BuildRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveContract_ExistingId_ReturnsNoContent()
    {
        var seeded = await SeedContractViaApiAsync("Archive Me");

        var response = await _client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveContract_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/contracts/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnarchiveContract_ExistingArchivedContract_ReturnsNoContent()
    {
        var seeded = await SeedContractViaApiAsync("Unarchive Me");
        await _client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        var response = await _client.PatchAsync($"/api/contracts/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetContracts_ArchivedHiddenByDefault()
    {
        var seeded = await SeedContractViaApiAsync("Hidden Contract");
        await _client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        var response = await _client.GetAsync("/api/contracts?search=Hidden+Contract");
        var result = await response.Content.ReadFromJsonAsync<PagedContracts>(JsonOptions);

        Assert.Equal(0, result!.Total);
    }

    [Fact]
    public async Task GetContracts_ArchivedShownWhenRequested()
    {
        var seeded = await SeedContractViaApiAsync("Archived Contract");
        await _client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        var response = await _client.GetAsync("/api/contracts?archived=true&search=Archived+Contract");
        var result = await response.Content.ReadFromJsonAsync<PagedContracts>(JsonOptions);

        Assert.Contains(result!.Items, contract => contract.Subject == "Archived Contract");
    }
}
