using Api.Common.Database;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.UserLeaveAllowances;

public class UserLeaveAllowanceRepositoryTests
{
    private static UserLeaveAllowanceRepository CreateRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new UserLeaveAllowanceRepository(context);
    }

    private static async Task<User> AddUserAsync(AppDbContext context, string name = "Alice")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = $"{name.ToLower()}-{Guid.NewGuid()}@test.com",
            Role = UserRole.User,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task<UserLeaveAllowance> AddAllowanceDirectlyAsync(
        AppDbContext context,
        Guid userId,
        int year = 2026,
        AllowanceMode mode = AllowanceMode.Limited,
        decimal totalDays = 20m)
    {
        var allowance = new UserLeaveAllowance
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeaveTypeId = Guid.NewGuid(),
            Year = year,
            Mode = mode,
            TotalDays = totalDays,
        };
        context.UserLeaveAllowances.Add(allowance);
        await context.SaveChangesAsync();
        return allowance;
    }

    [Fact]
    public async Task GetForUserAndYear_ReturnsAllowancesForUser()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context);
        await AddAllowanceDirectlyAsync(context, user.Id, year: 2026);
        await AddAllowanceDirectlyAsync(context, user.Id, year: 2026);

        var allowances = await repository.GetForUserAndYearAsync(user.Id, 2026);

        Assert.Equal(2, allowances.Count);
        Assert.All(allowances, allowance => Assert.Equal(user.Id, allowance.UserId));
    }

    [Fact]
    public async Task GetForUserAndYear_ExcludesOtherUsersAllowances()
    {
        var repository = CreateRepository(out var context);
        var alice = await AddUserAsync(context, "Alice");
        var bob = await AddUserAsync(context, "Bob");
        await AddAllowanceDirectlyAsync(context, alice.Id, year: 2026);
        await AddAllowanceDirectlyAsync(context, bob.Id, year: 2026);

        var allowances = await repository.GetForUserAndYearAsync(alice.Id, 2026);

        Assert.Single(allowances);
        Assert.Equal(alice.Id, allowances[0].UserId);
    }

    [Fact]
    public async Task GetForUserAndYear_ExcludesOtherYears()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context);
        await AddAllowanceDirectlyAsync(context, user.Id, year: 2025);
        await AddAllowanceDirectlyAsync(context, user.Id, year: 2026);

        var allowances = await repository.GetForUserAndYearAsync(user.Id, 2026);

        Assert.Single(allowances);
        Assert.Equal(2026, allowances[0].Year);
    }

    [Fact]
    public async Task GetForUserAndYear_NoAllowances_ReturnsEmpty()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context);

        var allowances = await repository.GetForUserAndYearAsync(user.Id, 2026);

        Assert.Empty(allowances);
    }

    [Fact]
    public async Task AddRangeAsync_PersistsAllowances()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context);
        var firstLeaveTypeId = Guid.NewGuid();
        var secondLeaveTypeId = Guid.NewGuid();
        var entities = new List<UserLeaveAllowance>
        {
            new() { Id = Guid.NewGuid(), UserId = user.Id, LeaveTypeId = firstLeaveTypeId, Year = 2026, Mode = AllowanceMode.Limited, TotalDays = 20m },
            new() { Id = Guid.NewGuid(), UserId = user.Id, LeaveTypeId = secondLeaveTypeId, Year = 2026, Mode = AllowanceMode.Unlimited, TotalDays = 0m },
        };

        await repository.AddRangeAsync(entities);

        var persisted = await context.UserLeaveAllowances
            .Where(allowance => allowance.UserId == user.Id)
            .ToListAsync();
        Assert.Equal(2, persisted.Count);
        Assert.Contains(persisted, allowance => allowance.LeaveTypeId == firstLeaveTypeId && allowance.TotalDays == 20m);
        Assert.Contains(persisted, allowance => allowance.LeaveTypeId == secondLeaveTypeId && allowance.Mode == AllowanceMode.Unlimited);
    }

    [Fact]
    public async Task AddRangeAsync_EmptyList_DoesNotPersistAnything()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context);

        await repository.AddRangeAsync([]);

        var persisted = await context.UserLeaveAllowances
            .Where(allowance => allowance.UserId == user.Id)
            .ToListAsync();
        Assert.Empty(persisted);
    }

    [Fact]
    public async Task RemoveAsync_RemovesById()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context);
        var allowance = await AddAllowanceDirectlyAsync(context, user.Id);

        await repository.RemoveAsync(allowance.Id);

        var stillExists = await context.UserLeaveAllowances.AnyAsync(item => item.Id == allowance.Id);
        Assert.False(stillExists);
    }

    [Fact]
    public async Task RemoveRangeAsync_RemovesMultiple()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context);
        var first = await AddAllowanceDirectlyAsync(context, user.Id);
        var second = await AddAllowanceDirectlyAsync(context, user.Id);
        var third = await AddAllowanceDirectlyAsync(context, user.Id);

        await repository.RemoveRangeAsync([first.Id, second.Id]);

        var remaining = await context.UserLeaveAllowances
            .Where(item => item.UserId == user.Id)
            .ToListAsync();
        Assert.Single(remaining);
        Assert.Equal(third.Id, remaining[0].Id);
    }
}
