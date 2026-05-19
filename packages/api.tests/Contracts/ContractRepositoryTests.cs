using Api.Common;
using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Contracts;

public class ContractRepositoryTests
{
    private static ContractRepository CreateRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new ContractRepository(context);
    }

    private static async Task<Guid> AddCustomerAsync(AppDbContext context, string name = "Test Customer", bool isArchived = false)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Number = 100000,
            Name = name,
            Country = "Belgium",
            IsArchived = isArchived,
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer.Id;
    }

    private static async Task<Guid> AddConsultantAsync(AppDbContext context, string name = "Consultant", UserRole role = UserRole.User, bool isArchived = false)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = $"consultant-{Guid.NewGuid()}@test.com",
            Role = role,
            IsArchived = isArchived,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private static async Task<Contract> AddContractAsync(
        AppDbContext context,
        Guid customerId,
        Guid consultantId,
        string subject = "Test Contract",
        int number = 100000,
        bool isArchived = false)
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Number = number,
            CustomerId = customerId,
            ConsultantId = consultantId,
            Subject = subject,
            StartDate = new DateOnly(2026, 1, 1),
            IsArchived = isArchived,
            Tasks = [new ContractTask { Id = Guid.NewGuid(), Name = "Task 1", DayRate = 800m, Order = 0 }],
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        return contract;
    }

    private static ContractRequest BuildRequest(
        Guid customerId,
        Guid consultantId,
        string subject = "Website Redesign",
        List<ContractTaskRequest>? tasks = null) => new()
    {
        CustomerId = customerId,
        ConsultantId = consultantId,
        Subject = subject,
        StartDate = new DateOnly(2026, 1, 1),
        EndDate = new DateOnly(2026, 6, 30),
        Tasks = tasks ?? [new ContractTaskRequest { Name = "Analysis", DayRate = 800m }],
    };

    [Fact]
    public async Task GetAll_ExcludesArchivedByDefault()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, number: 100001);
        await AddContractAsync(context, customerId, consultantId, number: 100002, isArchived: true);

        var (items, total) = await repository.GetAllAsync(null, ContractSort.Number, SortDirection.Asc, 1, 25, false);

        Assert.Equal(1, total);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetAll_IncludesArchivedWhenRequested()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, number: 100001);
        await AddContractAsync(context, customerId, consultantId, number: 100002, isArchived: true);

        var (items, total) = await repository.GetAllAsync(null, ContractSort.Number, SortDirection.Asc, 1, 25, true);

        Assert.Equal(2, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetAll_SearchMatchesSubject()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, subject: "Cloud Migration", number: 100001);
        await AddContractAsync(context, customerId, consultantId, subject: "Frontend Redesign", number: 100002);

        var (items, total) = await repository.GetAllAsync("cloud", ContractSort.Number, SortDirection.Asc, 1, 25, false);

        Assert.Equal(1, total);
        Assert.Equal("Cloud Migration", items[0].Subject);
    }

    [Fact]
    public async Task GetAll_SearchMatchesCustomerName()
    {
        var repository = CreateRepository(out var context);
        var acmeId = await AddCustomerAsync(context, "Acme Corp");
        var initechId = await AddCustomerAsync(context, "Initech");
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, acmeId, consultantId, number: 100001);
        await AddContractAsync(context, initechId, consultantId, number: 100002);

        var (items, total) = await repository.GetAllAsync("acme", ContractSort.Number, SortDirection.Asc, 1, 25, false);

        Assert.Equal(1, total);
        Assert.Equal(acmeId, items[0].CustomerId);
    }

    [Fact]
    public async Task GetAll_SearchMatchesConsultantName()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var aliceId = await AddConsultantAsync(context, "Alice Smith");
        var bobId = await AddConsultantAsync(context, "Bob Jones");
        await AddContractAsync(context, customerId, aliceId, number: 100001);
        await AddContractAsync(context, customerId, bobId, number: 100002);

        var (items, total) = await repository.GetAllAsync("alice", ContractSort.Number, SortDirection.Asc, 1, 25, false);

        Assert.Equal(1, total);
        Assert.Equal(aliceId, items[0].ConsultantId);
    }

    [Fact]
    public async Task GetAll_SortByNumberDesc_ReversesOrder()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, number: 100001);
        await AddContractAsync(context, customerId, consultantId, number: 100002);
        await AddContractAsync(context, customerId, consultantId, number: 100003);

        var (items, _) = await repository.GetAllAsync(null, ContractSort.Number, SortDirection.Desc, 1, 25, false);

        Assert.Equal(100003, items[0].Number);
        Assert.Equal(100001, items[2].Number);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        for (var index = 1; index <= 5; index++)
            await AddContractAsync(context, customerId, consultantId, number: 100000 + index);

        var (items, total) = await repository.GetAllAsync(null, ContractSort.Number, SortDirection.Asc, 2, 2, false);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsContract()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await AddContractAsync(context, customerId, consultantId, subject: "Alpha Project");

        var found = await repository.GetByIdAsync(contract.Id);

        Assert.NotNull(found);
        Assert.Equal("Alpha Project", found.Subject);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var found = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_AssignsNumberOne()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);

        var contract = await repository.CreateAsync(BuildRequest(customerId, consultantId));

        Assert.Equal(1, contract.Number);
    }

    [Fact]
    public async Task Create_AssignsSequentialNumbers()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, number: 5);

        var contract = await repository.CreateAsync(BuildRequest(customerId, consultantId));

        Assert.Equal(6, contract.Number);
    }

    [Fact]
    public async Task Create_CreatesTasks()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var tasks = new List<ContractTaskRequest>
        {
            new() { Name = "Design", DayRate = 700m },
            new() { Name = "Build", DayRate = 900m },
        };

        var contract = await repository.CreateAsync(BuildRequest(customerId, consultantId, tasks: tasks));

        Assert.Equal(2, contract.Tasks.Count(task => !task.IsArchived));
        Assert.Contains(contract.Tasks, task => task.Name == "Design");
        Assert.Contains(contract.Tasks, task => task.Name == "Build");
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await AddContractAsync(context, customerId, consultantId);

        var result = await repository.ArchiveAsync(contract.Id);
        var archived = await context.Contracts.FindAsync(contract.Id);

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
    public async Task Unarchive_ExistingArchivedContract_SetsIsArchivedFalse()
    {
        var repository = CreateRepository(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await AddContractAsync(context, customerId, consultantId, isArchived: true);

        var result = await repository.UnarchiveAsync(contract.Id);
        var unarchived = await context.Contracts.FindAsync(contract.Id);

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
