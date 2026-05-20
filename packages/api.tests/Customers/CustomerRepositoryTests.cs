using Api.Common;
using Api.Common.Database;
using Api.Modules.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Customers;

public class CustomerRepositoryShould
{
    private static CustomerRepository CreateRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
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
    public async Task Exclude_Archived_Customers_From_List()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Active", number: 100001);
        await AddCustomerAsync(context, "Archived", number: 100002, isArchived: true);

        var (items, total) = await repository.GetAllAsync(null, CustomerSort.Number, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Active", items[0].Name);
    }

    [Fact]
    public async Task Match_Name_In_Search()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Globex Corp", number: 100001);
        await AddCustomerAsync(context, "Initech", number: 100002);

        var (items, total) = await repository.GetAllAsync("globex", CustomerSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Globex Corp", items[0].Name);
    }

    [Fact]
    public async Task Match_Contact_Name_In_Search()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Globex", contactName: "Hank", number: 100001);
        await AddCustomerAsync(context, "Initech", contactName: "Peter", number: 100002);

        var (items, total) = await repository.GetAllAsync("peter", CustomerSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Initech", items[0].Name);
    }

    [Fact]
    public async Task Sort_By_Number_Descending()
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
    public async Task Sort_By_City_Alphabetically()
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
    public async Task Page_Customer_List()
    {
        var repository = CreateRepository(out var context);
        for (var index = 1; index <= 5; index++)
            await AddCustomerAsync(context, $"Customer {index}", number: 100000 + index);

        var (items, total) = await repository.GetAllAsync(null, CustomerSort.Number, SortDirection.Asc, 2, 2);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task Find_Customer_By_Id()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Alice Co");

        var found = await repository.GetByIdAsync(customer.Id);

        Assert.NotNull(found);
        Assert.Equal("Alice Co", found.Name);
    }

    [Fact]
    public async Task Return_Null_For_Unknown_Id()
    {
        var repository = CreateRepository(out _);

        var found = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_Customer_With_All_Fields()
    {
        var repository = CreateRepository(out _);
        var request = BuildRequest("Carol Co", "Carol", "carol@carol.test");

        var customer = await repository.CreateAsync(request);

        Assert.Equal("Carol Co", customer.Name);
        Assert.Equal("Carol", customer.ContactName);
        Assert.Equal("carol@carol.test", customer.ContactEmail);
    }

    [Fact]
    public async Task Assign_Number_One_To_First_Customer()
    {
        var repository = CreateRepository(out _);

        var customer = await repository.CreateAsync(BuildRequest());

        Assert.Equal(1, customer.Number);
    }

    [Fact]
    public async Task Assign_Sequential_Number()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Existing", number: 42);

        var customer = await repository.CreateAsync(BuildRequest("New", "New", "new@new.test"));

        Assert.Equal(43, customer.Number);
    }

    [Fact]
    public async Task Increment_From_Highest_Existing_Number()
    {
        var repository = CreateRepository(out var context);
        await AddCustomerAsync(context, "Existing", number: 5);

        var customer = await repository.CreateAsync(BuildRequest("New", "New", "new@new.test"));

        var persisted = await context.Customers.FindAsync(customer.Id);
        Assert.NotNull(persisted);
        Assert.Equal(6, persisted.Number);
    }

    [Fact]
    public async Task Update_Customer_Fields()
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
    public async Task Return_Null_On_Update_With_Unknown_Id()
    {
        var repository = CreateRepository(out _);

        var updated = await repository.UpdateAsync(Guid.NewGuid(), BuildRequest());

        Assert.Null(updated);
    }

    [Fact]
    public async Task Preserve_Number_On_Update()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Original", number: 100077);

        var updated = await repository.UpdateAsync(customer.Id, BuildRequest("Renamed"));

        Assert.NotNull(updated);
        Assert.Equal(100077, updated.Number);
    }

    [Fact]
    public async Task Archive_Customer()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Henry");

        var result = await repository.ArchiveAsync(customer.Id);
        var archived = await context.Customers.FindAsync(customer.Id);

        Assert.True(result);
        Assert.True(archived!.IsArchived);
    }

    [Fact]
    public async Task Return_False_On_Archive_With_Unknown_Id()
    {
        var repository = CreateRepository(out _);

        var result = await repository.ArchiveAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task Unarchive_Customer()
    {
        var repository = CreateRepository(out var context);
        var customer = await AddCustomerAsync(context, "Irene", isArchived: true);

        var result = await repository.UnarchiveAsync(customer.Id);
        var unarchived = await context.Customers.FindAsync(customer.Id);

        Assert.True(result);
        Assert.False(unarchived!.IsArchived);
    }

    [Fact]
    public async Task Return_False_On_Unarchive_With_Unknown_Id()
    {
        var repository = CreateRepository(out _);

        var result = await repository.UnarchiveAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
