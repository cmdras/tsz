using Api.Common.Database;
using Api.Modules.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Common;

public class QueryExtensionsShould
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static Customer BuildCustomer(int number) => new()
    {
        Id = Guid.NewGuid(),
        Number = number,
        Name = $"Customer {number}",
        Street = "Main 1",
        Zip = "1000",
        City = "Brussels",
        Country = "Belgium",
        ContactName = "Contact",
        ContactEmail = "contact@example.com",
    };

    [Fact]
    public async Task Given_MultipleItems_When_Paging_Then_TotalReflectsAllItems()
    {
        await using var context = CreateContext();
        for (var index = 1; index <= 5; index++)
            context.Customers.Add(BuildCustomer(index));
        await context.SaveChangesAsync();

        var (_, total) = await context.Customers
            .OrderBy(customer => customer.Number)
            .ToPagedResultAsync(page: 1, pageSize: 2);

        Assert.Equal(5, total);
    }

    [Fact]
    public async Task Given_MultipleItems_When_RequestingSecondPage_Then_ReturnsCorrectItems()
    {
        await using var context = CreateContext();
        for (var index = 1; index <= 5; index++)
            context.Customers.Add(BuildCustomer(index));
        await context.SaveChangesAsync();

        var (items, _) = await context.Customers
            .OrderBy(customer => customer.Number)
            .ToPagedResultAsync(page: 2, pageSize: 2);

        Assert.Equal(2, items.Count);
        Assert.Equal(3, items[0].Number);
        Assert.Equal(4, items[1].Number);
    }
}
