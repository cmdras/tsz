using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.TimeEntries;
using Api.Modules.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.TimeEntries;

public class WeekSubmitRepositoryShould : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TimeEntryRepository _repository;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly Monday = new(2026, 5, 18);
    private static readonly DateTime Now = new(2026, 5, 18, 17, 0, 0, DateTimeKind.Utc);

    public WeekSubmitRepositoryShould()
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

    private void SeedUser(Guid userId, string email = "test@example.com")
    {
        _context.Users.Add(new User { Id = userId, Name = "Test User", Email = email, Role = UserRole.User });
        _context.SaveChanges();
    }

    private Guid SeedContractTask()
    {
        var customerId = Guid.NewGuid();
        _context.Customers.Add(new Customer { Id = customerId, Number = 1, Name = "Customer" });

        var contractId = Guid.NewGuid();
        _context.Contracts.Add(new Contract
        {
            Id = contractId,
            Number = 1,
            CustomerId = customerId,
            ConsultantId = UserId,
            Subject = "Contract",
            StartDate = Monday,
        });

        var taskId = Guid.NewGuid();
        _context.ContractTasks.Add(new ContractTask { Id = taskId, ContractId = contractId, Name = "Task" });
        _context.SaveChanges();
        return taskId;
    }

    [Fact]
    public async Task Insert_WeekSubmission_On_Submit()
    {
        await _repository.SubmitWeekAsync(UserId, Monday, [], [], Now);

        var submission = await _context.WeekSubmissions.SingleOrDefaultAsync();
        Assert.NotNull(submission);
        Assert.Equal(UserId, submission.UserId);
        Assert.Equal(Monday, submission.WeekStart);
        Assert.Equal(Now, submission.SubmittedAt);
    }

    [Fact]
    public async Task Upsert_Cells_And_Insert_Submission_Atomically()
    {
        var taskId = SeedContractTask();
        var cell = new WeekCell(taskId, null, Monday, 8m);

        await _repository.SubmitWeekAsync(UserId, Monday, [cell], [], Now);

        var entryCount = await _context.TimeEntries.CountAsync();
        var submissionCount = await _context.WeekSubmissions.CountAsync();
        Assert.Equal(1, entryCount);
        Assert.Equal(1, submissionCount);
    }

    [Fact]
    public async Task Throw_On_Duplicate_Submission()
    {
        await _repository.SubmitWeekAsync(UserId, Monday, [], [], Now);

        await Assert.ThrowsAsync<WeekAlreadySubmittedException>(
            () => _repository.SubmitWeekAsync(UserId, Monday, [], [], Now));
    }

    [Fact]
    public async Task Not_Affect_Other_Users_Submission()
    {
        var otherUserId = Guid.NewGuid();
        SeedUser(otherUserId, "other@example.com");

        await _repository.SubmitWeekAsync(UserId, Monday, [], [], Now);
        await _repository.SubmitWeekAsync(otherUserId, Monday, [], [], Now);

        var count = await _context.WeekSubmissions.CountAsync();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task DeleteWeekSubmission_Returns_True_And_Removes_Row_When_Submission_Exists()
    {
        await _repository.SubmitWeekAsync(UserId, Monday, [], [], Now);

        var deleted = await _repository.DeleteWeekSubmissionAsync(UserId, Monday);

        Assert.True(deleted);
        var count = await _context.WeekSubmissions.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task DeleteWeekSubmission_Does_Not_Touch_Time_Entries()
    {
        var taskId = SeedContractTask();
        var cell = new WeekCell(taskId, null, Monday, 8m);
        await _repository.SubmitWeekAsync(UserId, Monday, [cell], [], Now);

        await _repository.DeleteWeekSubmissionAsync(UserId, Monday);

        var entryCount = await _context.TimeEntries.CountAsync();
        Assert.Equal(1, entryCount);
    }

    [Fact]
    public async Task DeleteWeekSubmission_Returns_False_When_No_Submission_Exists()
    {
        var deleted = await _repository.DeleteWeekSubmissionAsync(UserId, Monday);

        Assert.False(deleted);
    }
}
