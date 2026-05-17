using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Counters;
using Api.Common.Database;
using Api.Modules.Customers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Customers;

public class CustomerEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public CustomerEndpointsTests(WebApplicationFactory<Program> factory)
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
                    .UseInMemoryDatabase("CustomerIntegrationTests_" + guid)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            });
        });
        _client = _testFactory.CreateClient();

        using var scope = _testFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Counters.Add(new Counter { Key = CounterKeys.Customer, Value = 99999 });
        dbContext.SaveChanges();
    }

    private async Task<Customer> SeedCustomerViaApiAsync(
        string name = "Acme",
        string contactName = "Alice",
        string contactEmail = "alice@acme.test",
        string city = "Brussels")
    {
        var request = new CustomerRequest
        {
            Name = name,
            Street = "Main 1",
            Zip = "1000",
            City = city,
            Country = "Belgium",
            ContactName = contactName,
            ContactEmail = contactEmail,
        };
        var response = await _client.PostAsJsonAsync("/api/customers", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Customer>(JsonOptions))!;
    }

    [Fact]
    public async Task GetCustomers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/customers");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedCustomers>(JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCustomerById_ExistingId_ReturnsOk()
    {
        var seeded = await SeedCustomerViaApiAsync("Beta Corp", "Bob", "bob@beta.test");

        var response = await _client.GetAsync($"/api/customers/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<Customer>(JsonOptions);
        Assert.NotNull(customer);
        Assert.Equal("Beta Corp", customer.Name);
    }

    [Fact]
    public async Task GetCustomerById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_ValidRequest_ReturnsCreated()
    {
        var request = new CustomerRequest
        {
            Name = "Carol Co",
            Street = "Rue 7",
            Zip = "1050",
            City = "Brussels",
            Country = "Belgium",
            ContactName = "Carol",
            ContactEmail = "carol@carol.test",
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<Customer>(JsonOptions);
        Assert.NotNull(customer);
        Assert.Equal("Carol Co", customer.Name);
        Assert.True(customer.Number >= 100000);
    }

    [Fact]
    public async Task CreateCustomer_AssignsSequentialNumbers()
    {
        var first = await SeedCustomerViaApiAsync("First", "F", "f@first.test");
        var second = await SeedCustomerViaApiAsync("Second", "S", "s@second.test");

        Assert.Equal(first.Number + 1, second.Number);
    }

    [Fact]
    public async Task CreateCustomer_InvalidRequest_ReturnsBadRequest()
    {
        var request = new { Name = "", ContactEmail = "not-an-email" };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ValidRequest_ReturnsOk()
    {
        var seeded = await SeedCustomerViaApiAsync("Eve Ltd", "Eve", "eve@eve.test");
        var request = new CustomerRequest
        {
            Name = "Eve Updated",
            Street = "New 9",
            Zip = "2000",
            City = "Antwerp",
            Country = "Belgium",
            ContactName = "Eve B.",
            ContactEmail = "eve-updated@eve.test",
        };

        var response = await _client.PutAsJsonAsync($"/api/customers/{seeded.Id}", request);

        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<Customer>(JsonOptions);
        Assert.Equal("Eve Updated", customer!.Name);
        Assert.Equal("Antwerp", customer.City);
    }

    [Fact]
    public async Task UpdateCustomer_NonExistingId_ReturnsNotFound()
    {
        var request = new CustomerRequest
        {
            Name = "Ghost",
            ContactEmail = "ghost@ghost.test",
        };

        var response = await _client.PutAsJsonAsync($"/api/customers/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveCustomer_ExistingId_ReturnsNoContent()
    {
        var seeded = await SeedCustomerViaApiAsync("Henry Inc", "Henry", "henry@henry.test");

        var response = await _client.PatchAsync($"/api/customers/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveCustomer_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/customers/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnarchiveCustomer_ExistingArchivedCustomer_ReturnsNoContent()
    {
        var seeded = await SeedCustomerViaApiAsync("Irene Sa", "Irene", "irene@irene.test");
        await _client.PatchAsync($"/api/customers/{seeded.Id}/archive", null);

        var response = await _client.PatchAsync($"/api/customers/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_ArchivedCustomers_ExcludedFromList()
    {
        var seeded = await SeedCustomerViaApiAsync("Zephyr Bv", "Zee", "zee@zephyr.test");
        await _client.PatchAsync($"/api/customers/{seeded.Id}/archive", null);

        var response = await _client.GetAsync("/api/customers?search=zephyr");
        var result = await response.Content.ReadFromJsonAsync<PagedCustomers>(JsonOptions);

        Assert.Equal(0, result!.Total);
    }

    [Fact]
    public async Task GetCustomers_Search_FiltersByNameAndContact()
    {
        await SeedCustomerViaApiAsync("Globex Corp", "Hank", "hank@globex.test");
        await SeedCustomerViaApiAsync("Initech", "Peter", "peter@initech.test");

        var byName = await _client.GetFromJsonAsync<PagedCustomers>("/api/customers?search=globex", JsonOptions);
        var byContact = await _client.GetFromJsonAsync<PagedCustomers>("/api/customers?search=peter", JsonOptions);

        Assert.Contains(byName!.Items, customer => customer.Name == "Globex Corp");
        Assert.Contains(byContact!.Items, customer => customer.ContactName == "Peter");
    }
}
