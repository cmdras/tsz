using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Customers;

public static class CustomerSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Customers.AnyAsync())
            return;

        dbContext.Customers.AddRange(
            new Customer { Id = Guid.NewGuid(), Number = 100000, Name = "Alpha",   Street = "Keizerslaan 1",  Zip = "1000", City = "Brussel",   Country = "Belgium", ContactName = "Alice Alpha",   ContactEmail = "alice@alpha.be"   },
            new Customer { Id = Guid.NewGuid(), Number = 100001, Name = "Bravo",   Street = "Meir 2",         Zip = "2000", City = "Antwerpen", Country = "Belgium", ContactName = "Bob Bravo",     ContactEmail = "bob@bravo.be"     },
            new Customer { Id = Guid.NewGuid(), Number = 100002, Name = "Charlie", Street = "Veldstraat 3",   Zip = "9000", City = "Gent",      Country = "Belgium", ContactName = "Carol Charlie", ContactEmail = "carol@charlie.be" },
            new Customer { Id = Guid.NewGuid(), Number = 100003, Name = "Delta",   Street = "Steenweg 4",     Zip = "3000", City = "Leuven",    Country = "Belgium", ContactName = "Dave Delta",    ContactEmail = "dave@delta.be"    },
            new Customer { Id = Guid.NewGuid(), Number = 100004, Name = "Echo",    Street = "Lippenslaan 5",  Zip = "8300", City = "Knokke",    Country = "Belgium", ContactName = "Eve Echo",      ContactEmail = "eve@echo.be"      }
        );
        await dbContext.SaveChangesAsync();
    }
}
