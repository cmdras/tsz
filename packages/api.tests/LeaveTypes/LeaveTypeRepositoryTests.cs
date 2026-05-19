using Api.Common;
using Api.Common.Database;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.LeaveTypes;

public class LeaveTypeRepositoryTests
{
    private static LeaveTypeRepository CreateRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new LeaveTypeRepository(context);
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

    private static LeaveTypeRequest BuildRequest(
        string name = "Holiday",
        decimal defaultDays = 20m,
        AllowanceMode defaultMode = AllowanceMode.Limited) => new()
    {
        Name = name,
        DefaultDays = defaultDays,
        DefaultMode = defaultMode,
    };

    [Fact]
    public async Task GetAll_ExcludesArchivedLeaveTypes()
    {
        var repository = CreateRepository(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        await AddLeaveTypeAsync(context, "Sick Leave", isArchived: true);

        var (items, total) = await repository.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: false);

        Assert.Equal(1, total);
        Assert.Equal("Holiday", items[0].Name);
    }

    [Fact]
    public async Task GetAll_ShowArchived_IncludesArchivedLeaveTypes()
    {
        var repository = CreateRepository(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        await AddLeaveTypeAsync(context, "Sick Leave", isArchived: true);

        var (items, total) = await repository.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: true);

        Assert.Equal(2, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetAll_SearchMatchesName()
    {
        var repository = CreateRepository(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        await AddLeaveTypeAsync(context, "ADV");

        var (items, total) = await repository.GetAllAsync("holi", LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: false);

        Assert.Equal(1, total);
        Assert.Equal("Holiday", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SortByNameAsc_OrdersAlphabetically()
    {
        var repository = CreateRepository(out var context);
        await AddLeaveTypeAsync(context, "Sickness");
        await AddLeaveTypeAsync(context, "ADV");
        await AddLeaveTypeAsync(context, "Holiday");

        var (items, _) = await repository.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: false);

        Assert.Equal("ADV", items[0].Name);
        Assert.Equal("Holiday", items[1].Name);
        Assert.Equal("Sickness", items[2].Name);
    }

    [Fact]
    public async Task GetAll_SortByDefaultDaysDesc_ReversesOrder()
    {
        var repository = CreateRepository(out var context);
        await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m);
        await AddLeaveTypeAsync(context, "ADV", defaultDays: 5m);

        var (items, _) = await repository.GetAllAsync(null, LeaveTypeSort.DefaultDays, SortDirection.Desc, 1, 25, showArchived: false);

        Assert.Equal(20m, items[0].DefaultDays);
        Assert.Equal(5m, items[1].DefaultDays);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var repository = CreateRepository(out var context);
        for (var index = 1; index <= 5; index++)
            await AddLeaveTypeAsync(context, $"Leave Type {index}");

        var (items, total) = await repository.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 2, 2, showArchived: false);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsLeaveType()
    {
        var repository = CreateRepository(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday");

        var found = await repository.GetByIdAsync(leaveType.Id);

        Assert.NotNull(found);
        Assert.Equal("Holiday", found.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var found = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesLeaveType()
    {
        var repository = CreateRepository(out _);

        var leaveType = await repository.CreateAsync(BuildRequest("Holiday", 20m));

        Assert.Equal("Holiday", leaveType.Name);
        Assert.Equal(20m, leaveType.DefaultDays);
        Assert.False(leaveType.IsArchived);
    }

    [Fact]
    public async Task Update_ValidRequest_UpdatesLeaveType()
    {
        var repository = CreateRepository(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m);

        var updated = await repository.UpdateAsync(leaveType.Id, BuildRequest("Holiday Updated", 25m));

        Assert.NotNull(updated);
        Assert.Equal("Holiday Updated", updated.Name);
        Assert.Equal(25m, updated.DefaultDays);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var updated = await repository.UpdateAsync(Guid.NewGuid(), BuildRequest());

        Assert.Null(updated);
    }

    [Fact]
    public async Task ExistsByName_ExistingName_ReturnsTrue()
    {
        var repository = CreateRepository(out var context);
        await AddLeaveTypeAsync(context, "Holiday");

        var exists = await repository.ExistsByNameAsync("Holiday");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByName_CaseInsensitive_ReturnsTrue()
    {
        var repository = CreateRepository(out var context);
        await AddLeaveTypeAsync(context, "Holiday");

        var exists = await repository.ExistsByNameAsync("HOLIDAY");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByName_ExcludeId_ExcludesSelf()
    {
        var repository = CreateRepository(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday");

        var exists = await repository.ExistsByNameAsync("Holiday", excludeId: leaveType.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByName_UnknownName_ReturnsFalse()
    {
        var repository = CreateRepository(out _);

        var exists = await repository.ExistsByNameAsync("Unknown");

        Assert.False(exists);
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var repository = CreateRepository(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday");

        var result = await repository.ArchiveAsync(leaveType.Id);
        var archived = await context.LeaveTypes.FindAsync(leaveType.Id);

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
    public async Task Unarchive_ExistingId_SetsIsArchivedFalse()
    {
        var repository = CreateRepository(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday", isArchived: true);

        var result = await repository.UnarchiveAsync(leaveType.Id);
        var unarchived = await context.LeaveTypes.FindAsync(leaveType.Id);

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
