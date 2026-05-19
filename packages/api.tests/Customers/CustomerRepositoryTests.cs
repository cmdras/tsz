using Api.Common;
using Api.Common.Counters;
using Api.Common.Database;
using Api.Modules.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Customers;

public class CustomerRepositoryTests
{
    private static CustomerRepository CreateRepository(out AppDbContext context, int initialCounterValue = 99999)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        context.Counters.Add(new Counter { Key = CounterKeys.Customer, Value = initialCounterValue });
        context.SaveChanges();
        return new CustomerRepository(context);
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
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Active", number: 100001);
        await AddCustomerAsync(context, "Archived", number: 100002, isArchived: true);

        var (items, total) = await repository.GetAllAsync(null, CustomerSort.Number, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Active", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesName()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Globex Corp", number: 100001);
        await AddCustomerAsync(context, "Initech", number: 100002);

        var (items, total) = await repository.GetAllAsync("globex", CustomerSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Globex Corp", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesContactName()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Globex", contactName: "Hank", number: 100001);
        await AddCustomerAsync(context, "Initech", contactName: "Peter", number: 100002);

        var (items, total) = await repository.GetAllAsync("peter", CustomerSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Initech", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SortByNumberDesc_ReversesOrder()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Alpha", number: 100001);
        await AddCustomerAsync(context, "Beta", number: 100002);
        await AddCustomerAsync(context, "Gamma", number: 100003);

        var (items, _) = await repository.GetAllAsync(null, CustomerSort.Number, SortDirection.Desc, 1, 25);

        Assert.Equal(100003, items[0].Number);
        Assert.Equal(100002, items[1].Number);
        Assert.Equal(100001, items[2].Number);
    }

    [Fact]
    public async Task GetAll_SortByCity_OrdersAlphabetically()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Alpha", city: "Ghent", number: 100001);
        await AddCustomerAsync(context, "Beta", city: "Antwerp", number: 100002);
        await AddCustomerAsync(context, "Gamma", city: "Brussels", number: 100003);

        var (items, _) = await repository.GetAllAsync(null, CustomerSort.City, SortDirection.Asc, 1, 25);

        Assert.Equal("Antwerp", items[0].City);
        Assert.Equal("Brussels", items[1].City);
        Assert.Equal("Ghent", items[2].City);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var repository = CreateRepository(out var context);
        for (var index = 1; index <= 5; index++)
            await AddCustomerAsync(context, $"Customer {index}", number: 100000 + index);

        var (items, total) = await repository.GetAllAsync(null, CustomerSort.Number, SortDirection.Asc, 2, 2);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCustomer()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Alice Co");

        var found = await repository.GetByIdAsync(customer.Id);

        Assert.NotNull(found);
        Assert.Equal("Alice Co", found.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var found = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesCustomerWithFields()
    {
        var repository = CreateRepository(out _);
        var request = BuildRequest("Carol Co", "Carol", "carol@carol.test");

        var customer = await repository.CreateAsync(request);

        Assert.Equal("Carol Co", customer.Name);
        Assert.Equal("Carol", customer.ContactName);
        Assert.Equal("carol@carol.test", customer.ContactEmail);
    }

    [Fact]
    public async Task Create_FirstCustomer_GetsNumber100000()
    {
        var repository = CreateRepository(out _);

        var customer = await repository.CreateAsync(BuildRequest());

        Assert.Equal(100000, customer.Number);
    }

    [Fact]
    public async Task Create_AssignsSequentialNumbers()
    {
        var repository = CreateRepository(out var context, initialCounterValue: 100042);
        await AddCustomerAsync(context, "Existing", number: 100042);

        var customer = await repository.CreateAsync(BuildRequest("New", "New", "new@new.test"));

        Assert.Equal(100043, customer.Number);
    }

    [Fact]
    public async Task Create_IncrementsCounterAndPersistsCustomerTogether()
    {
        var repository = CreateRepository(out var context);

        var customer = await repository.CreateAsync(BuildRequest());

        var counter = await context.Counters.FindAsync(CounterKeys.Customer);
        var persisted = await context.Customers.FindAsync(customer.Id);
        Assert.NotNull(persisted);
        Assert.Equal(customer.Number, counter!.Value);
    }

    [Fact]
    public async Task Update_ValidRequest_UpdatesFields()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Eve Ltd", contactEmail: "eve@eve.test");
        var request = BuildRequest("Eve Updated", "Eve B.", "eve-updated@eve.test", city: "Antwerp");

        var updated = await repository.UpdateAsync(customer.Id, request);

        Assert.NotNull(updated);
        Assert.Equal("Eve Updated", updated.Name);
        Assert.Equal("Antwerp", updated.City);
        Assert.Equal("eve-updated@eve.test", updated.ContactEmail);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var updated = await repository.UpdateAsync(Guid.NewGuid(), BuildRequest());

        Assert.Null(updated);
    }

    [Fact]
    public async Task Update_PreservesNumber()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Original", number: 100077);

        var updated = await repository.UpdateAsync(customer.Id, BuildRequest("Renamed"));

        Assert.NotNull(updated);
        Assert.Equal(100077, updated.Number);
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Henry");

        var result = await repository.ArchiveAsync(customer.Id);
        var archived = await context.Customers.FindAsync(customer.Id);

        Assert.True(result);
        Assert.True(archived!.IsArchived);
    }

    [Fact]
    public async Task Archive_NonExistingId_ReturnsFalse()
    {
        var repository = CreateRepository(out _);

        var result = await repository.ArchiveAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task Unarchive_ExistingId_SetsIsArchivedFalse()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Irene", isArchived: true);

        var result = await repository.UnarchiveAsync(customer.Id);
        var unarchived = await context.Customers.FindAsync(customer.Id);

        Assert.True(result);
        Assert.False(unarchived!.IsArchived);
    }

    [Fact]
    public async Task Unarchive_NonExistingId_ReturnsFalse()
    {
        var repository = CreateRepository(out _);

        var result = await repository.UnarchiveAsync(Guid.NewGuid());

        Assert.False(result);
    }
}

public class CustomerResponseTests
{
    [Fact]
    public void FromEntity_MapsAllFields()
    {
        var customer = new Customer
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Number = 100001,
            Name = "Acme",
            Street = "Main 1",
            Zip = "1000",
            City = "Brussels",
            Country = "Belgium",
            ContactName = "Alice",
            ContactEmail = "alice@acme.test",
            IsArchived = false,
        };

        var response = CustomerResponse.FromEntity(customer);

        Assert.Equal(customer.Id, response.Id);
        Assert.Equal(customer.Number, response.Number);
        Assert.Equal(customer.Name, response.Name);
        Assert.Equal(customer.Street, response.Street);
        Assert.Equal(customer.Zip, response.Zip);
        Assert.Equal(customer.City, response.City);
        Assert.Equal(customer.Country, response.Country);
        Assert.Equal(customer.ContactName, response.ContactName);
        Assert.Equal(customer.ContactEmail, response.ContactEmail);
        Assert.Equal(customer.IsArchived, response.IsArchived);
    }
}
