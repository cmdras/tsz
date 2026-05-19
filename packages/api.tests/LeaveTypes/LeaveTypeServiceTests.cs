using Api.Common;
using Api.Common.Database;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.LeaveTypes;

public class LeaveTypeServiceTests
{
    private static LeaveTypeService CreateService(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new LeaveTypeService(new LeaveTypeRepository(context));
    }

    private static async Task<LeaveType> AddLeaveTypeAsync(
        AppDbContext context,
        string name,
        decimal defaultDays = 0m,
        bool isArchived = false)
    {
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = name,
            DefaultDays = defaultDays,
            IsArchived = isArchived,
        };
        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync();
        return leaveType;
    }

    private static LeaveTypeRequest BuildRequest(
        string name = "Holiday",
        decimal defaultDays = 20m) => new()
    {
        Name = name,
        DefaultDays = defaultDays,
    };

    [Fact]
    public async Task GetAll_ExcludesArchivedLeaveTypes()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        await AddLeaveTypeAsync(context, "Sick Leave", isArchived: true);

        var result = await service.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: false);

        Assert.Equal(1, result.Total);
        Assert.Equal("Holiday", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_ShowArchived_IncludesArchivedLeaveTypes()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        await AddLeaveTypeAsync(context, "Sick Leave", isArchived: true);

        var result = await service.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: true);

        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task GetAll_SearchMatchesName()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        await AddLeaveTypeAsync(context, "ADV");

        var result = await service.GetAllAsync("holi", LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: false);

        Assert.Equal(1, result.Total);
        Assert.Equal("Holiday", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SortByNameAsc_OrdersAlphabetically()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Sickness");
        await AddLeaveTypeAsync(context, "ADV");
        await AddLeaveTypeAsync(context, "Holiday");

        var result = await service.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 1, 25, showArchived: false);

        Assert.Equal("ADV", result.Items[0].Name);
        Assert.Equal("Holiday", result.Items[1].Name);
        Assert.Equal("Sickness", result.Items[2].Name);
    }

    [Fact]
    public async Task GetAll_SortByNameDesc_ReversesOrder()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "ADV");
        await AddLeaveTypeAsync(context, "Holiday");

        var result = await service.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Desc, 1, 25, showArchived: false);

        Assert.Equal("Holiday", result.Items[0].Name);
        Assert.Equal("ADV", result.Items[1].Name);
    }

    [Fact]
    public async Task GetAll_SortByDefaultDaysAsc_OrdersByDays()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m);
        await AddLeaveTypeAsync(context, "ADV", defaultDays: 5m);
        await AddLeaveTypeAsync(context, "Sickness", defaultDays: 0m);

        var result = await service.GetAllAsync(null, LeaveTypeSort.DefaultDays, SortDirection.Asc, 1, 25, showArchived: false);

        Assert.Equal(0m, result.Items[0].DefaultDays);
        Assert.Equal(5m, result.Items[1].DefaultDays);
        Assert.Equal(20m, result.Items[2].DefaultDays);
    }

    [Fact]
    public async Task GetAll_SortByDefaultDaysDesc_ReversesOrder()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m);
        await AddLeaveTypeAsync(context, "ADV", defaultDays: 5m);

        var result = await service.GetAllAsync(null, LeaveTypeSort.DefaultDays, SortDirection.Desc, 1, 25, showArchived: false);

        Assert.Equal(20m, result.Items[0].DefaultDays);
        Assert.Equal(5m, result.Items[1].DefaultDays);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var service = CreateService(out var context);
        for (var index = 1; index <= 5; index++)
            await AddLeaveTypeAsync(context, $"Leave Type {index}");

        var result = await service.GetAllAsync(null, LeaveTypeSort.Name, SortDirection.Asc, 2, 2, showArchived: false);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsLeaveType()
    {
        var service = CreateService(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday");

        var found = await service.GetByIdAsync(leaveType.Id);

        Assert.NotNull(found);
        Assert.Equal("Holiday", found.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        var service = CreateService(out _);

        var found = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesLeaveType()
    {
        var service = CreateService(out _);
        var request = BuildRequest("Holiday", 20m);

        var leaveType = await service.CreateAsync(request);

        Assert.Equal("Holiday", leaveType.Name);
        Assert.Equal(20m, leaveType.DefaultDays);
        Assert.False(leaveType.IsArchived);
    }

    [Fact]
    public async Task Create_DuplicateName_ThrowsDuplicateLeaveTypeNameException()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");

        await Assert.ThrowsAsync<DuplicateLeaveTypeNameException>(() =>
            service.CreateAsync(BuildRequest("Holiday")));
    }

    [Fact]
    public async Task Create_DuplicateNameDifferentCase_ThrowsDuplicateLeaveTypeNameException()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");

        await Assert.ThrowsAsync<DuplicateLeaveTypeNameException>(() =>
            service.CreateAsync(BuildRequest("HOLIDAY")));
    }

    [Fact]
    public async Task Update_ValidRequest_UpdatesLeaveType()
    {
        var service = CreateService(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m);
        var request = BuildRequest("Holiday Updated", 25m);

        var updated = await service.UpdateAsync(leaveType.Id, request);

        Assert.NotNull(updated);
        Assert.Equal("Holiday Updated", updated.Name);
        Assert.Equal(25m, updated.DefaultDays);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNull()
    {
        var service = CreateService(out _);

        var updated = await service.UpdateAsync(Guid.NewGuid(), BuildRequest());

        Assert.Null(updated);
    }

    [Fact]
    public async Task Update_SameName_Succeeds()
    {
        var service = CreateService(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday", defaultDays: 20m);
        var request = BuildRequest("Holiday", 25m);

        var updated = await service.UpdateAsync(leaveType.Id, request);

        Assert.NotNull(updated);
        Assert.Equal(25m, updated.DefaultDays);
    }

    [Fact]
    public async Task Update_DuplicateName_ThrowsDuplicateLeaveTypeNameException()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        var adv = await AddLeaveTypeAsync(context, "ADV");

        await Assert.ThrowsAsync<DuplicateLeaveTypeNameException>(() =>
            service.UpdateAsync(adv.Id, BuildRequest("Holiday")));
    }

    [Fact]
    public async Task Update_DuplicateNameDifferentCase_ThrowsDuplicateLeaveTypeNameException()
    {
        var service = CreateService(out var context);
        await AddLeaveTypeAsync(context, "Holiday");
        var adv = await AddLeaveTypeAsync(context, "ADV");

        await Assert.ThrowsAsync<DuplicateLeaveTypeNameException>(() =>
            service.UpdateAsync(adv.Id, BuildRequest("HOLIDAY")));
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var service = CreateService(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday");

        var result = await service.ArchiveAsync(leaveType.Id);
        var archived = await context.LeaveTypes.FindAsync(leaveType.Id);

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
    public async Task Unarchive_ExistingId_SetsIsArchivedFalse()
    {
        var service = CreateService(out var context);
        var leaveType = await AddLeaveTypeAsync(context, "Holiday", isArchived: true);

        var result = await service.UnarchiveAsync(leaveType.Id);
        var unarchived = await context.LeaveTypes.FindAsync(leaveType.Id);

        Assert.True(result);
        Assert.False(unarchived!.IsArchived);
    }

    [Fact]
    public async Task Unarchive_NonExistingId_ReturnsFalse()
    {
        var service = CreateService(out _);

        var result = await service.UnarchiveAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task Create_DefaultModeLimited_PersistsLimited()
    {
        var service = CreateService(out _);
        var request = new LeaveTypeRequest
        {
            Name = "Holiday",
            DefaultDays = 20m,
            DefaultMode = AllowanceMode.Limited,
        };

        var leaveType = await service.CreateAsync(request);

        Assert.Equal(AllowanceMode.Limited, leaveType.DefaultMode);
    }

    [Fact]
    public async Task Update_DefaultMode_PersistsNewMode()
    {
        var service = CreateService(out var context);
        var seeded = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "Holiday",
            DefaultDays = 20m,
            DefaultMode = AllowanceMode.Unlimited,
        };
        context.LeaveTypes.Add(seeded);
        await context.SaveChangesAsync();
        var request = new LeaveTypeRequest
        {
            Name = "Holiday",
            DefaultDays = 20m,
            DefaultMode = AllowanceMode.Limited,
        };

        var updated = await service.UpdateAsync(seeded.Id, request);

        Assert.NotNull(updated);
        Assert.Equal(AllowanceMode.Limited, updated.DefaultMode);
    }

    [Fact]
    public async Task Seeder_SicknessIsUnlimited_OthersAreLimited()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using var context = new AppDbContext(options);

        await LeaveTypeSeeder.SeedAsync(context);

        var sickness = await context.LeaveTypes.SingleAsync(leaveType => leaveType.Name == "Sickness");
        Assert.Equal(AllowanceMode.Unlimited, sickness.DefaultMode);

        var otherNames = new[] { "Holiday", "ADV", "Ancienniteit", "Holiday replacement" };
        foreach (var name in otherNames)
        {
            var leaveType = await context.LeaveTypes.SingleAsync(item => item.Name == name);
            Assert.Equal(AllowanceMode.Limited, leaveType.DefaultMode);
        }
    }
}
