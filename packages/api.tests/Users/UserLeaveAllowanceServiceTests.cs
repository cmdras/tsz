using Api.Common.Database;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Users;

public class UserLeaveAllowanceServiceShould
{
    private static UserService CreateService(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new UserService(new UserRepository(context), new UserLeaveAllowanceRepository(context), new LeaveTypeRepository(context));
    }

    private static async Task<LeaveType> AddLeaveTypeAsync(
        AppDbContext context,
        string name,
        decimal defaultDays = 20m,
        AllowanceMode defaultMode = AllowanceMode.Limited,
        bool isArchived = false)
    {
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = name,
            DefaultDays = defaultDays,
            DefaultMode = defaultMode,
            IsArchived = isArchived,
        };
        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync();
        return leaveType;
    }

    private static UserRequest BuildRequest(
        string name = "Alice",
        string email = "alice@example.com",
        UserRole role = UserRole.User,
        List<UserLeaveAllowanceRequest>? leaves = null) => new()
    {
        Name = name,
        Email = email,
        Role = role,
        Leaves = leaves ?? [],
    };

    [Fact]
    public async Task Populate_Current_Year_Allowances_On_Create()
    {
        var service = CreateService(out var context);
        var holiday = await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m, defaultMode: AllowanceMode.Limited);
        var sickness = await AddLeaveTypeAsync(context, "Sickness", defaultDays: 0m, defaultMode: AllowanceMode.Unlimited);

        var created = await service.CreateAsync(BuildRequest());

        var allowances = await context.UserLeaveAllowances
            .Where(allowance => allowance.UserId == created.Id)
            .ToListAsync();
        Assert.Equal(2, allowances.Count);
        Assert.Contains(allowances, allowance =>
            allowance.LeaveTypeId == holiday.Id &&
            allowance.Mode == AllowanceMode.Limited &&
            allowance.TotalDays == 20m &&
            allowance.Year == DateTime.UtcNow.Year);
        Assert.Contains(allowances, allowance =>
            allowance.LeaveTypeId == sickness.Id &&
            allowance.Mode == AllowanceMode.Unlimited &&
            allowance.TotalDays == 0m);
    }

    [Fact]
    public async Task Exclude_Archived_Leave_Types_When_Populating_Allowances()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        await AddLeaveTypeAsync(context, "ObsoleteType", isArchived: true);

        var created = await service.CreateAsync(BuildRequest());

        var allowances = await context.UserLeaveAllowances
            .Where(allowance => allowance.UserId == created.Id)
            .ToListAsync();
        Assert.Single(allowances);
    }

    [Fact]
    public async Task Create_No_Allowances_When_No_Leave_Types_Exist()
    {
        var service = CreateService(out var context);

        var created = await service.CreateAsync(BuildRequest());

        var allowances = await context.UserLeaveAllowances
            .Where(allowance => allowance.UserId == created.Id)
            .ToListAsync();
        Assert.Empty(allowances);
    }

    [Fact]
    public async Task Include_Current_Year_Leaves_In_Response()
    {
        var service = CreateService(out var context);
        var holiday = await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m, defaultMode: AllowanceMode.Limited);
        var created = await service.CreateAsync(BuildRequest());

        var response = await service.GetByIdAsync(created.Id);

        Assert.NotNull(response);
        Assert.Single(response.Leaves);
        var entry = response.Leaves[0];
        Assert.Equal(holiday.Id, entry.LeaveTypeId);
        Assert.Equal("Holiday", entry.Name);
        Assert.Equal(AllowanceMode.Limited, entry.Mode);
        Assert.Equal(20m, entry.TotalDays);
        Assert.Equal(0m, entry.Taken);
        Assert.Equal(20m, entry.Balance);
    }

    [Fact]
    public async Task Return_Null_Balance_For_Unlimited_Mode()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Sickness", defaultDays: 0m, defaultMode: AllowanceMode.Unlimited);
        var created = await service.CreateAsync(BuildRequest());

        var response = await service.GetByIdAsync(created.Id);

        Assert.NotNull(response);
        var entry = Assert.Single(response.Leaves);
        Assert.Equal(AllowanceMode.Unlimited, entry.Mode);
        Assert.Null(entry.Balance);
    }

    [Fact]
    public async Task Update_Existing_Leave_Allowance()
    {
        var service = CreateService(out var context);
        var holiday = await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m, defaultMode: AllowanceMode.Limited);
        var created = await service.CreateAsync(BuildRequest());
        var existing = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == created.Id);

        var updateRequest = BuildRequest(
            name: created.Name,
            email: created.Email,
            role: created.Role,
            leaves: [
                new UserLeaveAllowanceRequest
                {
                    Id = existing.Id,
                    LeaveTypeId = holiday.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 25m,
                },
            ]);

        await service.UpdateAsync(created.Id, updateRequest);

        var refreshed = await context.UserLeaveAllowances
            .AsNoTracking()
            .FirstAsync(allowance => allowance.Id == existing.Id);
        Assert.Equal(25m, refreshed.TotalDays);
        Assert.Equal(AllowanceMode.Limited, refreshed.Mode);
    }

    [Fact]
    public async Task Delete_Allowance_Removed_From_Request()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        var created = await service.CreateAsync(BuildRequest());
        var existing = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == created.Id);

        var updateRequest = BuildRequest(
            name: created.Name,
            email: created.Email,
            role: created.Role,
            leaves: []);

        await service.UpdateAsync(created.Id, updateRequest);

        var stillExists = await context.UserLeaveAllowances
            .AsNoTracking()
            .AnyAsync(allowance => allowance.Id == existing.Id);
        Assert.False(stillExists);
    }

    [Fact]
    public async Task Insert_New_Allowance_With_Null_Id()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        var created = await service.CreateAsync(BuildRequest());
        var lateAdded = await AddLeaveTypeAsync(context, "ADV", defaultDays: 5m, defaultMode: AllowanceMode.Limited);
        var existing = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == created.Id);

        var updateRequest = BuildRequest(
            name: created.Name,
            email: created.Email,
            role: created.Role,
            leaves: [
                new UserLeaveAllowanceRequest
                {
                    Id = existing.Id,
                    LeaveTypeId = existing.LeaveTypeId,
                    Mode = existing.Mode,
                    TotalDays = existing.TotalDays,
                },
                new UserLeaveAllowanceRequest
                {
                    Id = null,
                    LeaveTypeId = lateAdded.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 5m,
                },
            ]);

        await service.UpdateAsync(created.Id, updateRequest);

        var allowances = await context.UserLeaveAllowances
            .AsNoTracking()
            .Where(allowance => allowance.UserId == created.Id)
            .ToListAsync();
        Assert.Equal(2, allowances.Count);
        Assert.Contains(allowances, allowance =>
            allowance.LeaveTypeId == lateAdded.Id &&
            allowance.TotalDays == 5m &&
            allowance.Mode == AllowanceMode.Limited);
    }

    [Fact]
    public async Task Reject_Duplicate_Leave_Type_For_Same_Year()
    {
        var service = CreateService(out var context);
        var holiday = await AddLeaveTypeAsync(context, "Holiday");
        var created = await service.CreateAsync(BuildRequest());
        var existing = await context.UserLeaveAllowances.FirstAsync(allowance => allowance.UserId == created.Id);

        var updateRequest = BuildRequest(
            name: created.Name,
            email: created.Email,
            role: created.Role,
            leaves: [
                new UserLeaveAllowanceRequest
                {
                    Id = existing.Id,
                    LeaveTypeId = holiday.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 20m,
                },
                new UserLeaveAllowanceRequest
                {
                    Id = null,
                    LeaveTypeId = holiday.Id,
                    Mode = AllowanceMode.Limited,
                    TotalDays = 15m,
                },
            ]);

        await Assert.ThrowsAsync<DuplicateUserLeaveAllowanceException>(() =>
            service.UpdateAsync(created.Id, updateRequest));
    }
}
