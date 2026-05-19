using Api.Common.Database;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Contracts;

public static class ContractSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Contracts.AnyAsync())
            return;

        var customers = await dbContext.Customers.Take(5).ToListAsync();
        var consultants = await dbContext.Users
            .Where(user => !user.IsArchived && user.Role != UserRole.ClientManager)
            .Take(3)
            .ToListAsync();

        if (customers.Count < 5 || consultants.Count < 3)
            return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        dbContext.Contracts.AddRange(
            new Contract
            {
                Id = Guid.NewGuid(), Number = 1,
                CustomerId = customers[0].Id, ConsultantId = consultants[0].Id,
                Subject = "Digital Transformation",
                StartDate = today, EndDate = today.AddMonths(6),
                Tasks =
                [
                    new ContractTask { Id = Guid.NewGuid(), Name = "Analysis", DayRate = 800m, Order = 0 },
                    new ContractTask { Id = Guid.NewGuid(), Name = "Development", DayRate = 900m, Order = 1 },
                ],
            },
            new Contract
            {
                Id = Guid.NewGuid(), Number = 2,
                CustomerId = customers[1].Id, ConsultantId = consultants[1].Id,
                Subject = "Cloud Migration",
                StartDate = today.AddMonths(-1), EndDate = today.AddMonths(5),
                Tasks =
                [
                    new ContractTask { Id = Guid.NewGuid(), Name = "Architecture", DayRate = 950m, Order = 0 },
                    new ContractTask { Id = Guid.NewGuid(), Name = "Implementation", DayRate = 850m, Order = 1 },
                ],
            },
            new Contract
            {
                Id = Guid.NewGuid(), Number = 3,
                CustomerId = customers[2].Id, ConsultantId = consultants[2].Id,
                Subject = "ERP Integration",
                StartDate = today.AddMonths(-3),
                Tasks =
                [
                    new ContractTask { Id = Guid.NewGuid(), Name = "Discovery", DayRate = 750m, Order = 0 },
                    new ContractTask { Id = Guid.NewGuid(), Name = "Configuration", DayRate = 800m, Order = 1 },
                ],
            },
            new Contract
            {
                Id = Guid.NewGuid(), Number = 4,
                CustomerId = customers[3].Id, ConsultantId = consultants[0].Id,
                Subject = "Data Warehouse",
                StartDate = today.AddMonths(-2), EndDate = today.AddMonths(4),
                Tasks =
                [
                    new ContractTask { Id = Guid.NewGuid(), Name = "Modelling", DayRate = 875m, Order = 0 },
                    new ContractTask { Id = Guid.NewGuid(), Name = "ETL Pipelines", DayRate = 925m, Order = 1 },
                ],
            },
            new Contract
            {
                Id = Guid.NewGuid(), Number = 5,
                CustomerId = customers[4].Id, ConsultantId = consultants[1].Id,
                Subject = "Security Audit",
                StartDate = today, EndDate = today.AddMonths(2),
                Tasks =
                [
                    new ContractTask { Id = Guid.NewGuid(), Name = "Penetration Testing", DayRate = 1000m, Order = 0 },
                    new ContractTask { Id = Guid.NewGuid(), Name = "Remediation", DayRate = 850m, Order = 1 },
                ],
            }
        );

        await dbContext.SaveChangesAsync();
    }
}
