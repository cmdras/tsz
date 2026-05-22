using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.TimeEntries;
using Api.Modules.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.TimeEntries;

public class TimeEntryRepositoryUpsertShould : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TimeEntryRepository _repository;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly Monday = new(2026, 5, 18);
    private static readonly DateTime Now = new(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);

    public TimeEntryRepositoryUpsertShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new TimeEntryRepository(_context);
        SeedUser(UserId);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private void SeedUser(Guid userId)
    {
        _context.Users.Add(new User { Id = userId, Name = "Test User", Email = "test@example.com", Role = UserRole.User });
        _context.SaveChanges();
    }

    private Guid SeedContractTask()
    {
        var customerId = Guid.NewGuid();
        _context.Customers.Add(new Customer { Id = customerId, Number = 1, Name = "Customer" });

        var contractId = Guid.NewGuid();
        _context.Contracts.Add(new Contract
        {
            Id = contractId, Number = 1, CustomerId = customerId, ConsultantId = UserId,
            Subject = "Contract", StartDate = Monday,
        });

        var taskId = Guid.NewGuid();
        _context.ContractTasks.Add(new ContractTask { Id = taskId, ContractId = contractId, Name = "Task" });

        _context.SaveChanges();
        return taskId;
    }

    // --- GetWeekEntries ---

    [Fact]
    public async Task Return_Empty_List_When_No_Entries_In_Week()
    {
        var entries = await _repository.GetWeekEntriesAsync(UserId, Monday);

        Assert.Empty(entries);
    }

    [Fact]
    public async Task Return_Entries_Within_The_Week()
    {
        var taskId = SeedContractTask();
        _context.TimeEntries.Add(new TimeEntry
        {
            Id = Guid.NewGuid(), UserId = UserId, Date = Monday,
            ContractTaskId = taskId, Hours = 8m, UpdatedAt = Now,
        });
        await _context.SaveChangesAsync();

        var entries = await _repository.GetWeekEntriesAsync(UserId, Monday);

        Assert.Single(entries);
        Assert.Equal(taskId, entries[0].ContractTaskId);
    }

    [Fact]
    public async Task Exclude_Entries_Outside_The_Week()
    {
        var taskId = SeedContractTask();
        var nextMonday = Monday.AddDays(7);
        _context.TimeEntries.Add(new TimeEntry
        {
            Id = Guid.NewGuid(), UserId = UserId, Date = nextMonday,
            ContractTaskId = taskId, Hours = 8m, UpdatedAt = Now,
        });
        await _context.SaveChangesAsync();

        var entries = await _repository.GetWeekEntriesAsync(UserId, Monday);

        Assert.Empty(entries);
    }

    // --- ApplyWeekDiff: upsert ---

    [Fact]
    public async Task Insert_New_Cell_When_Not_Present()
    {
        var taskId = SeedContractTask();
        var toUpsert = new[] { new WeekCell(ContractTaskId: taskId, LeaveTypeId: null, Date: Monday, Hours: 8m) };

        await _repository.ApplyWeekDiffAsync(UserId, toUpsert, [], Now);

        var entries = await _repository.GetWeekEntriesAsync(UserId, Monday);
        Assert.Single(entries);
        Assert.Equal(8m, entries[0].Hours);
        Assert.Equal(Now, entries[0].UpdatedAt);
    }

    [Fact]
    public async Task Update_Hours_When_Cell_Already_Exists()
    {
        var taskId = SeedContractTask();
        var entryId = Guid.NewGuid();
        _context.TimeEntries.Add(new TimeEntry
        {
            Id = entryId, UserId = UserId, Date = Monday,
            ContractTaskId = taskId, Hours = 8m, UpdatedAt = Now,
        });
        await _context.SaveChangesAsync();

        var later = Now.AddHours(1);
        var toUpsert = new[] { new WeekCell(ContractTaskId: taskId, LeaveTypeId: null, Date: Monday, Hours: 4m) };

        await _repository.ApplyWeekDiffAsync(UserId, toUpsert, [], later);

        var entries = await _repository.GetWeekEntriesAsync(UserId, Monday);
        Assert.Single(entries);
        Assert.Equal(4m, entries[0].Hours);
        Assert.Equal(later, entries[0].UpdatedAt);
    }

    // --- ApplyWeekDiff: delete ---

    [Fact]
    public async Task Delete_Entry_By_Id()
    {
        var taskId = SeedContractTask();
        var entryId = Guid.NewGuid();
        _context.TimeEntries.Add(new TimeEntry
        {
            Id = entryId, UserId = UserId, Date = Monday,
            ContractTaskId = taskId, Hours = 8m, UpdatedAt = Now,
        });
        await _context.SaveChangesAsync();

        await _repository.ApplyWeekDiffAsync(UserId, [], [entryId], Now);

        var entries = await _repository.GetWeekEntriesAsync(UserId, Monday);
        Assert.Empty(entries);
    }

    // --- Round-trip ---

    [Fact]
    public async Task Round_Trip_Upsert_Then_GetWeek_Returns_LastSavedAt()
    {
        var taskId = SeedContractTask();
        var toUpsert = new[] { new WeekCell(ContractTaskId: taskId, LeaveTypeId: null, Date: Monday, Hours: 8m) };

        await _repository.ApplyWeekDiffAsync(UserId, toUpsert, [], Now);

        var weekData = await _repository.GetWeekAsync(UserId, Monday);
        Assert.Equal(Now, weekData.LastSavedAt);
    }

    [Fact]
    public async Task Round_Trip_Clearing_Cell_Deletes_DB_Row()
    {
        var taskId = SeedContractTask();
        var toUpsert = new[] { new WeekCell(ContractTaskId: taskId, LeaveTypeId: null, Date: Monday, Hours: 8m) };
        await _repository.ApplyWeekDiffAsync(UserId, toUpsert, [], Now);

        var entries = await _repository.GetWeekEntriesAsync(UserId, Monday);
        await _repository.ApplyWeekDiffAsync(UserId, [], entries.Select(entry => entry.Id).ToList(), Now);

        var entriesAfterClear = await _repository.GetWeekEntriesAsync(UserId, Monday);
        Assert.Empty(entriesAfterClear);

        var weekData = await _repository.GetWeekAsync(UserId, Monday);
        Assert.Null(weekData.LastSavedAt);
    }
}
