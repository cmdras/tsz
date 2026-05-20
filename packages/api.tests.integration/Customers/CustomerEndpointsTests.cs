using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Modules.Customers;
using Api.Tests.Integration.Common;
using Api.Tests.Integration.Common.Builders;

namespace Api.Tests.Integration.Customers;

public class CustomerEndpointsShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<CustomerResponse> SeedAsync(CustomerBuilder builder)
    {
        var response = await factory.Client.PostAsJsonAsync("/api/customers", builder.Build());
        response.EnsureSuccessStatusCode();
        var customer = (await response.Content.ReadFromJsonAsync<CustomerResponse>(IntegrationFactory.JsonOptions))!;
        if (builder.IsArchived)
            await factory.Client.PatchAsync($"/api/customers/{customer.Id}/archive", null);
        return customer;
    }

    [Fact]
    public async Task Return_Ok_For_Customer_List()
    {
        var response = await factory.Client.GetAsync("/api/customers");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedCustomers>(IntegrationFactory.JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Find_Customer_By_Id()
    {
        var seeded = await SeedAsync(CustomerBuilder.Globex);

        var response = await factory.Client.GetAsync($"/api/customers/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(IntegrationFactory.JsonOptions);
        Assert.NotNull(customer);
        Assert.Equal("Globex Corp", customer.Name);
    }

    [Fact]
    public async Task Return_Not_Found_For_Unknown_Customer()
    {
        var response = await factory.Client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Customer_And_Return_Created()
    {
        var response = await factory.Client.PostAsJsonAsync("/api/customers", CustomerBuilder.Acme.Build());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(IntegrationFactory.JsonOptions);
        Assert.NotNull(customer);
        Assert.Equal("Acme", customer.Name);
        Assert.True(customer.Number >= 1);
    }

    [Fact]
    public async Task Assign_Sequential_Numbers_On_Create()
    {
        var first = await SeedAsync(CustomerBuilder.Globex);
        var second = await SeedAsync(CustomerBuilder.Acme);

        Assert.Equal(first.Number + 1, second.Number);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Invalid_Customer()
    {
        var request = new { Name = "", ContactEmail = "not-an-email" };

        var response = await factory.Client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_Customer_Fields()
    {
        var seeded = await SeedAsync(CustomerBuilder.Acme);
        var request = new CustomerRequest
        {
            Name = "Acme Updated",
            Street = "New 9",
            Zip = "2000",
            City = "Antwerp",
            Country = "Belgium",
            ContactName = "Bob",
            ContactEmail = "bob@acme.test",
        };

        var response = await factory.Client.PutAsJsonAsync($"/api/customers/{seeded.Id}", request);

        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(IntegrationFactory.JsonOptions);
        Assert.Equal("Acme Updated", customer!.Name);
        Assert.Equal("Antwerp", customer.City);
    }

    [Fact]
    public async Task Return_Not_Found_On_Update_With_Unknown_Id()
    {
        var request = new CustomerRequest { Name = "Ghost", ContactEmail = "ghost@ghost.test" };

        var response = await factory.Client.PutAsJsonAsync($"/api/customers/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Archive_Customer()
    {
        var seeded = await SeedAsync(CustomerBuilder.Globex);

        var response = await factory.Client.PatchAsync($"/api/customers/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Return_Not_Found_On_Archive_With_Unknown_Id()
    {
        var response = await factory.Client.PatchAsync($"/api/customers/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unarchive_Customer()
    {
        var seeded = await SeedAsync(CustomerBuilder.Globex.Archived());

        var response = await factory.Client.PatchAsync($"/api/customers/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Exclude_Archived_Customers_From_List()
    {
        var seeded = await SeedAsync(CustomerBuilder.Acme.Archived());

        var response = await factory.Client.GetAsync($"/api/customers?search=Acme");
        var result = await response.Content.ReadFromJsonAsync<PagedCustomers>(IntegrationFactory.JsonOptions);

        Assert.Equal(0, result!.Total);
    }

    [Fact]
    public async Task Filter_Customer_List_By_Name_And_Contact()
    {
        await SeedAsync(CustomerBuilder.Globex);
        await SeedAsync(CustomerBuilder.Acme.Named("Initech"));

        var byName = await factory.Client.GetFromJsonAsync<PagedCustomers>("/api/customers?search=globex", IntegrationFactory.JsonOptions);
        var byContact = await factory.Client.GetFromJsonAsync<PagedCustomers>("/api/customers?search=Contact", IntegrationFactory.JsonOptions);

        Assert.Contains(byName!.Items, customer => customer.Name == "Globex Corp");
        Assert.True(byContact!.Total > 0);
    }

    [Fact]
    public async Task RoundTrip_Sort_And_Pagination_Query_String()
    {
        for (var index = 1; index <= 4; index++)
            await SeedAsync(new CustomerBuilder().Named($"Company {index:D2}"));

        var page1 = await factory.Client.GetFromJsonAsync<PagedCustomers>(
            "/api/customers?sort=Name&sortDirection=Asc&page=1&pageSize=2",
            IntegrationFactory.JsonOptions);
        var page2 = await factory.Client.GetFromJsonAsync<PagedCustomers>(
            "/api/customers?sort=Name&sortDirection=Asc&page=2&pageSize=2",
            IntegrationFactory.JsonOptions);

        Assert.Equal(4, page1!.Total);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(2, page2!.Items.Count);
        Assert.True(string.Compare(page1.Items[0].Name, page1.Items[1].Name, StringComparison.Ordinal) < 0);
        Assert.True(string.Compare(page1.Items.Last().Name, page2.Items[0].Name, StringComparison.Ordinal) < 0);
    }
}
