using Api.Common;
using Api.Common.Counters;
using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Contracts;

public class ContractServiceTests
{
    private static ContractService CreateService(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        context.Counters.Add(new Counter { Key = CounterKeys.Contract, Value = 99999 });
        context.SaveChanges();
        var counterService = new CounterService(context);
        return new ContractService(context, counterService);
    }

    private static async Task<Guid> AddConsultantAsync(AppDbContext context, UserRole role = UserRole.User, bool isArchived = false)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Consultant",
            Email = $"consultant-{Guid.NewGuid()}@test.com",
            Role = role,
            IsArchived = isArchived,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private static async Task<Guid> AddCustomerAsync(AppDbContext context, bool isArchived = false)
    {
        var customer = new Api.Modules.Customers.Customer
        {
            Id = Guid.NewGuid(),
            Number = 100000,
            Name = "Test Customer",
            Country = "Belgium",
            IsArchived = isArchived,
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer.Id;
    }

    private static async Task<Contract> AddContractAsync(
        AppDbContext context,
        Guid customerId,
        Guid consultantId,
        int number = 100000,
        bool isArchived = false)
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Number = number,
            CustomerId = customerId,
            ConsultantId = consultantId,
            Subject = "Test Contract",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsArchived = isArchived,
            Tasks =
            [
                new ContractTask { Id = Guid.NewGuid(), Name = "Task 1", DayRate = 800m, Order = 0 },
            ],
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
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, number: 100001);
        await AddContractAsync(context, customerId, consultantId, number: 100002, isArchived: true);

        var result = await service.GetAllAsync(null, ContractSort.Number, SortDirection.Asc, 1, 25, false);

        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task GetAll_IncludesArchivedWhenRequested()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, number: 100001);
        await AddContractAsync(context, customerId, consultantId, number: 100002, isArchived: true);

        var result = await service.GetAllAsync(null, ContractSort.Number, SortDirection.Asc, 1, 25, true);

        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task GetAll_SearchMatchesSubject()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        await AddContractAsync(context, customerId, consultantId, number: 100001);
        context.Contracts.First().Subject = "Cloud Migration";
        await context.SaveChangesAsync();

        var result = await service.GetAllAsync("cloud", ContractSort.Number, SortDirection.Asc, 1, 25, false);

        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Create_ValidRequest_AssignsNumberFromCounter()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);

        var contract = await service.CreateAsync(BuildRequest(customerId, consultantId));

        Assert.Equal(100000, contract.Number);
    }

    [Fact]
    public async Task Create_AssignsSequentialNumbers()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);

        var first = await service.CreateAsync(BuildRequest(customerId, consultantId, "First"));
        var second = await service.CreateAsync(BuildRequest(customerId, consultantId, "Second"));

        Assert.Equal(first.Number + 1, second.Number);
    }

    [Fact]
    public async Task Create_CreatesTasks()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var tasks = new List<ContractTaskRequest>
        {
            new() { Name = "Design", DayRate = 700m },
            new() { Name = "Build", DayRate = 900m },
        };

        var contract = await service.CreateAsync(BuildRequest(customerId, consultantId, tasks: tasks));

        Assert.Equal(2, contract.Tasks.Count(task => !task.IsArchived));
        Assert.Contains(contract.Tasks, task => task.Name == "Design");
        Assert.Contains(contract.Tasks, task => task.Name == "Build");
    }

    [Fact]
    public async Task Create_NoTasks_ThrowsInvalidContractRequestException()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var request = BuildRequest(customerId, consultantId, tasks: []);

        await Assert.ThrowsAsync<InvalidContractRequestException>(
            () => service.CreateAsync(request));
    }

    [Fact]
    public async Task Create_EndDateBeforeStartDate_ThrowsInvalidContractRequestException()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var request = BuildRequest(customerId, consultantId);
        request.StartDate = new DateOnly(2026, 6, 1);
        request.EndDate = new DateOnly(2026, 1, 1);

        await Assert.ThrowsAsync<InvalidContractRequestException>(
            () => service.CreateAsync(request));
    }

    [Fact]
    public async Task Create_ClientManagerConsultant_ThrowsInvalidContractRequestException()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context, UserRole.ClientManager);

        await Assert.ThrowsAsync<InvalidContractRequestException>(
            () => service.CreateAsync(BuildRequest(customerId, consultantId)));
    }

    [Fact]
    public async Task Create_ArchivedConsultant_ThrowsInvalidContractRequestException()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context, isArchived: true);

        await Assert.ThrowsAsync<InvalidContractRequestException>(
            () => service.CreateAsync(BuildRequest(customerId, consultantId)));
    }

    [Fact]
    public async Task Create_MissingCustomer_ThrowsInvalidContractRequestException()
    {
        var service = CreateService(out var context);
        var consultantId = await AddConsultantAsync(context);

        await Assert.ThrowsAsync<InvalidContractRequestException>(
            () => service.CreateAsync(BuildRequest(Guid.NewGuid(), consultantId)));
    }

    [Fact]
    public async Task Create_ArchivedCustomer_ThrowsInvalidContractRequestException()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context, isArchived: true);
        var consultantId = await AddConsultantAsync(context);

        await Assert.ThrowsAsync<InvalidContractRequestException>(
            () => service.CreateAsync(BuildRequest(customerId, consultantId)));
    }

    [Fact]
    public async Task Update_ExistingTask_UpdatesInPlace()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await service.CreateAsync(BuildRequest(customerId, consultantId));
        var existingTaskId = contract.Tasks.First().Id;

        var updateRequest = BuildRequest(customerId, consultantId);
        updateRequest.Tasks = [new ContractTaskRequest { Id = existingTaskId, Name = "Updated Name", DayRate = 950m }];

        var updated = await service.UpdateAsync(contract.Id, updateRequest);

        Assert.NotNull(updated);
        var task = updated.Tasks.First(task => task.Id == existingTaskId);
        Assert.Equal("Updated Name", task.Name);
        Assert.Equal(950m, task.DayRate);
        Assert.False(task.IsArchived);
    }

    [Fact]
    public async Task Update_OmittedTask_ArchivesIt()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await service.CreateAsync(BuildRequest(customerId, consultantId));
        var taskId = contract.Tasks.First().Id;

        var updateRequest = BuildRequest(customerId, consultantId);
        updateRequest.Tasks = [new ContractTaskRequest { Name = "New Task", DayRate = 700m }];

        var updated = await service.UpdateAsync(contract.Id, updateRequest);

        Assert.NotNull(updated);
        Assert.True(updated.Tasks.First(task => task.Id == taskId).IsArchived);
        Assert.Contains(updated.Tasks, task => task.Name == "New Task" && !task.IsArchived);
    }

    [Fact]
    public async Task Update_UnknownTaskId_ThrowsInvalidContractRequestException()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await service.CreateAsync(BuildRequest(customerId, consultantId));

        var updateRequest = BuildRequest(customerId, consultantId);
        updateRequest.Tasks = [new ContractTaskRequest { Id = Guid.NewGuid(), Name = "Stranger", DayRate = 700m }];

        await Assert.ThrowsAsync<InvalidContractRequestException>(
            () => service.UpdateAsync(contract.Id, updateRequest));
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNull()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);

        var result = await service.UpdateAsync(Guid.NewGuid(), BuildRequest(customerId, consultantId));

        Assert.Null(result);
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await AddContractAsync(context, customerId, consultantId);

        var result = await service.ArchiveAsync(contract.Id);
        var archived = await context.Contracts.FindAsync(contract.Id);

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
    public async Task Unarchive_ExistingArchivedContract_SetsIsArchivedFalse()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await AddContractAsync(context, customerId, consultantId, isArchived: true);

        var result = await service.UnarchiveAsync(contract.Id);
        var unarchived = await context.Contracts.FindAsync(contract.Id);

        Assert.True(result);
        Assert.False(unarchived!.IsArchived);
    }

    [Fact]
    public async Task Archive_DoesNotCascadeToTasks()
    {
        var service = CreateService(out var context);
        var customerId = await AddCustomerAsync(context);
        var consultantId = await AddConsultantAsync(context);
        var contract = await AddContractAsync(context, customerId, consultantId);

        await service.ArchiveAsync(contract.Id);

        var tasks = await context.ContractTasks.Where(task => task.ContractId == contract.Id).ToListAsync();
        Assert.All(tasks, task => Assert.False(task.IsArchived));
    }

}
