using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Database;
using Api.Modules.Customers;
using Api.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Customers;

public class CustomerEndpointsTests : IClassFixture<CustomerApiFactory>, IAsyncLifetime
{
    private readonly CustomerApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public CustomerEndpointsTests(CustomerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Customers.RemoveRange(context.Customers);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<CustomerResponse> SeedCustomerViaApiAsync(
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
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions))!;
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
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
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
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        Assert.NotNull(customer);
        Assert.Equal("Carol Co", customer.Name);
        Assert.True(customer.Number >= 1);
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
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
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

public class CustomerApiFactory : TestApiFactory
{
    public CustomerApiFactory() : base("CustomerIntegrationTests") { }
}
