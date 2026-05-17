using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Common.Counters;

public static class CounterSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Counters.AnyAsync())
            return;

        var maxCustomerNumber = await dbContext.Customers.MaxAsync(customer => (int?)customer.Number) ?? 99999;
        var maxContractNumber = await dbContext.Contracts.MaxAsync(contract => (int?)contract.Number) ?? 99999;

        dbContext.Counters.AddRange(
            new Counter { Key = CounterKeys.Customer, Value = maxCustomerNumber },
            new Counter { Key = CounterKeys.Contract, Value = maxContractNumber }
        );

        await dbContext.SaveChangesAsync();
    }
}
