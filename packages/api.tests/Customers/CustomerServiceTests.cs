using Api.Common;
using Api.Common.Database;
using Api.Modules.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Customers;

public class CustomerServiceTests
{
    private static CustomerService CreateService(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new CustomerService(context);
    }

    private static async Task<Customer> AddCustomerAsync(
        AppDbContext context,
        string name,
        string contactName = "Contact",
        string contactEmail = "contact@example.com",
        string city = "Brussels",
        int number = 100000,
        bool isArchived = false)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Number = number,
            Name = name,
            Street = "Main 1",
            Zip = "1000",
            City = city,
            Country = "Belgium",
            ContactName = contactName,
            ContactEmail = contactEmail,
            IsArchived = isArchived,
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    private static CustomerRequest BuildRequest(
        string name = "Acme",
        string contactName = "Alice",
        string contactEmail = "alice@acme.test",
        string city = "Brussels") => new()
    {
        Name = name,
        Street = "Main 1",
        Zip = "1000",
        City = city,
        Country = "Belgium",
        ContactName = contactName,
        ContactEmail = contactEmail,
    };

    [Fact]
    public async Task GetAll_ExcludesArchivedCustomers()
    {
        var service = CreateService(out var context);
        await AddCustomerAsync(context, "Active", number: 100001);
        await AddCustomerAsync(context, "Archived", number: 100002, isArchived: true);

        var result = await service.GetAllAsync(null, CustomerSort.Number, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal("Active", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesName()
    {
        var service = CreateService(out var context);
        await AddCustomerAsync(context, "Globex Corp", number: 100001);
        await AddCustomerAsync(context, "Initech", number: 100002);

        var result = await service.GetAllAsync("globex", CustomerSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal("Globex Corp", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesContactName()
    {
        var service = CreateService(out var context);
        await AddCustomerAsync(context, "Globex", contactName: "Hank", number: 100001);
        await AddCustomerAsync(context, "Initech", contactName: "Peter", number: 100002);

        var result = await service.GetAllAsync("peter", CustomerSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal("Initech", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SortByNumberDesc_ReversesOrder()
    {
        var service = CreateService(out var context);
        await AddCustomerAsync(context, "Alpha", number: 100001);
        await AddCustomerAsync(context, "Beta", number: 100002);
        await AddCustomerAsync(context, "Gamma", number: 100003);

        var result = await service.GetAllAsync(null, CustomerSort.Number, SortDirection.Desc, 1, 25);

        Assert.Equal(100003, result.Items[0].Number);
        Assert.Equal(100002, result.Items[1].Number);
        Assert.Equal(100001, result.Items[2].Number);
    }

    [Fact]
    public async Task GetAll_SortByCity_OrdersAlphabetically()
    {
        var service = CreateService(out var context);
        await AddCustomerAsync(context, "Alpha", city: "Ghent", number: 100001);
        await AddCustomerAsync(context, "Beta", city: "Antwerp", number: 100002);
        await AddCustomerAsync(context, "Gamma", city: "Brussels", number: 100003);

        var result = await service.GetAllAsync(null, CustomerSort.City, SortDirection.Asc, 1, 25);

        Assert.Equal("Antwerp", result.Items[0].City);
        Assert.Equal("Brussels", result.Items[1].City);
        Assert.Equal("Ghent", result.Items[2].City);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var service = CreateService(out var context);
        for (var index = 1; index <= 5; index++)
            await AddCustomerAsync(context, $"Customer {index}", number: 100000 + index);

        var result = await service.GetAllAsync(null, CustomerSort.Number, SortDirection.Asc, 2, 2);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCustomer()
    {
        var service = CreateService(out var context);
        var customer = await AddCustomerAsync(context, "Alice Co");

        var found = await service.GetByIdAsync(customer.Id);

        Assert.NotNull(found);
        Assert.Equal("Alice Co", found.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        var service = CreateService(out _);

        var found = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesCustomer()
    {
        var service = CreateService(out _);
        var request = BuildRequest("Carol Co", "Carol", "carol@carol.test");

        var customer = await service.CreateAsync(request);

        Assert.Equal("Carol Co", customer.Name);
        Assert.Equal("Carol", customer.ContactName);
        Assert.Equal("carol@carol.test", customer.ContactEmail);
    }

    [Fact]
    public async Task Create_FirstCustomer_GetsNumber100000()
    {
        var service = CreateService(out _);

        var customer = await service.CreateAsync(BuildRequest());

        Assert.Equal(100000, customer.Number);
    }

    [Fact]
    public async Task Create_AssignsSequentialNumbers()
    {
        var service = CreateService(out var context);
        await AddCustomerAsync(context, "Existing", number: 100042);

        var customer = await service.CreateAsync(BuildRequest("New", "New", "new@new.test"));

        Assert.Equal(100043, customer.Number);
    }

    [Fact]
    public async Task Update_ValidRequest_UpdatesCustomer()
    {
        var service = CreateService(out var context);
        var customer = await AddCustomerAsync(context, "Eve Ltd", contactEmail: "eve@eve.test");
        var request = BuildRequest("Eve Updated", "Eve B.", "eve-updated@eve.test", city: "Antwerp");

        var updated = await service.UpdateAsync(customer.Id, request);

        Assert.NotNull(updated);
        Assert.Equal("Eve Updated", updated.Name);
        Assert.Equal("Antwerp", updated.City);
        Assert.Equal("eve-updated@eve.test", updated.ContactEmail);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNull()
    {
        var service = CreateService(out _);

        var updated = await service.UpdateAsync(Guid.NewGuid(), BuildRequest());

        Assert.Null(updated);
    }

    [Fact]
    public async Task Update_PreservesNumber()
    {
        var service = CreateService(out var context);
        var customer = await AddCustomerAsync(context, "Original", number: 100077);

        var updated = await service.UpdateAsync(customer.Id, BuildRequest("Renamed"));

        Assert.NotNull(updated);
        Assert.Equal(100077, updated.Number);
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var service = CreateService(out var context);
        var customer = await AddCustomerAsync(context, "Henry");

        var result = await service.ArchiveAsync(customer.Id);
        var archived = await context.Customers.FindAsync(customer.Id);

        Assert.True(result);
        Assert.True(archived!.IsArchived);
    }

    [Fact]
    public async Task Archive_NonExistingId_ReturnsFalse()
    {
        var service = CreateService(out _);

        var result = await service.ArchiveAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task Unarchive_ExistingId_SetsIsArchivedFalse()
    {
        var service = CreateService(out var context);
        var customer = await AddCustomerAsync(context, "Irene", isArchived: true);

        var result = await service.UnarchiveAsync(customer.Id);
        var unarchived = await context.Customers.FindAsync(customer.Id);

        Assert.True(result);
        Assert.False(unarchived!.IsArchived);
    }

    [Fact]
    public async Task Unarchive_NonExistingId_ReturnsFalse()
    {
        var service = CreateService(out _);

        var result = await service.UnarchiveAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
