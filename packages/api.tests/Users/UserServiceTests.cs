using Api.Common;
using Api.Common.Database;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Users;

public class UserServiceTests
{
    private static UserService CreateService(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        context = new AppDbContext(options);
        return new UserService(context);
    }

    private static async Task<User> AddUserAsync(AppDbContext context, string name, string email, UserRole role, bool isArchived = false)
    {
        var user = new User { Id = Guid.NewGuid(), Name = name, Email = email, Role = role, IsArchived = isArchived };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GetAll_ExcludesArchivedUsers()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Active", "active@example.com", UserRole.User);
        await AddUserAsync(context, "Archived", "archived@example.com", UserRole.User, isArchived: true);

        var result = await service.GetAllAsync(null, UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal("Active", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesName()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Alice Smith", "alice@example.com", UserRole.User);
        await AddUserAsync(context, "Bob Jones", "bob@example.com", UserRole.User);

        var result = await service.GetAllAsync("alice", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal("Alice Smith", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesEmail()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Alice", "alice@company.com", UserRole.User);
        await AddUserAsync(context, "Bob", "bob@other.com", UserRole.User);

        var result = await service.GetAllAsync("company", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal("Alice", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesRole()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Admin User", "admin@example.com", UserRole.Admin);
        await AddUserAsync(context, "Regular", "regular@example.com", UserRole.User);

        var result = await service.GetAllAsync("admin", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal("Admin User", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesRoleDisplayLabelWithSpace()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Mike", "mike@example.com", UserRole.ClientManager);
        await AddUserAsync(context, "Regular", "regular@example.com", UserRole.User);

        var result = await service.GetAllAsync("Client Manager", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, result.Total);
        Assert.Equal(UserRole.ClientManager, result.Items[0].Role);
    }

    [Fact]
    public async Task GetAll_SortByRole_UsesLogicalOrder()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Zara User", "zara@example.com", UserRole.User);
        await AddUserAsync(context, "Alice Admin", "alice@example.com", UserRole.Admin);
        await AddUserAsync(context, "Mike Manager", "mike@example.com", UserRole.ClientManager);

        var result = await service.GetAllAsync(null, UserSort.Role, SortDirection.Asc, 1, 25);

        Assert.Equal(3, result.Total);
        Assert.Equal(UserRole.Admin, result.Items[0].Role);
        Assert.Equal(UserRole.ClientManager, result.Items[1].Role);
        Assert.Equal(UserRole.User, result.Items[2].Role);
    }

    [Fact]
    public async Task GetAll_SortByRoleDesc_ReversesLogicalOrder()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Zara User", "zara@example.com", UserRole.User);
        await AddUserAsync(context, "Alice Admin", "alice@example.com", UserRole.Admin);
        await AddUserAsync(context, "Mike Manager", "mike@example.com", UserRole.ClientManager);

        var result = await service.GetAllAsync(null, UserSort.Role, SortDirection.Desc, 1, 25);

        Assert.Equal(UserRole.User, result.Items[0].Role);
        Assert.Equal(UserRole.ClientManager, result.Items[1].Role);
        Assert.Equal(UserRole.Admin, result.Items[2].Role);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var service = CreateService(out var context);
        for (var i = 1; i <= 5; i++)
            await AddUserAsync(context, $"User {i}", $"user{i}@example.com", UserRole.User);

        var result = await service.GetAllAsync(null, UserSort.Name, SortDirection.Asc, 2, 2);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsUser()
    {
        var service = CreateService(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com", UserRole.Admin);

        var found = await service.GetByIdAsync(user.Id);

        Assert.NotNull(found);
        Assert.Equal("Alice", found.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        var service = CreateService(out _);

        var found = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesUser()
    {
        var service = CreateService(out _);
        var request = new UserRequest { Name = "Alice", Email = "alice@example.com", Role = UserRole.Admin };

        var user = await service.CreateAsync(request);

        Assert.Equal("Alice", user.Name);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public async Task Create_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        var service = CreateService(out var context);
        await AddUserAsync(context, "Alice", "alice@example.com", UserRole.User);
        var request = new UserRequest { Name = "Alice2", Email = "alice@example.com", Role = UserRole.User };

        await Assert.ThrowsAsync<DuplicateEmailException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task Update_ValidRequest_UpdatesUser()
    {
        var service = CreateService(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com", UserRole.User);
        var request = new UserRequest { Name = "Alice Updated", Email = "alice2@example.com", Role = UserRole.Admin };

        var updated = await service.UpdateAsync(user.Id, request);

        Assert.NotNull(updated);
        Assert.Equal("Alice Updated", updated.Name);
        Assert.Equal("alice2@example.com", updated.Email);
        Assert.Equal(UserRole.Admin, updated.Role);
    }

    [Fact]
    public async Task Update_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        var service = CreateService(out var context);
        var alice = await AddUserAsync(context, "Alice", "alice@example.com", UserRole.User);
        await AddUserAsync(context, "Bob", "bob@example.com", UserRole.User);
        var request = new UserRequest { Name = "Alice", Email = "bob@example.com", Role = UserRole.User };

        await Assert.ThrowsAsync<DuplicateEmailException>(() => service.UpdateAsync(alice.Id, request));
    }

    [Fact]
    public async Task Update_SameEmailSelf_Succeeds()
    {
        var service = CreateService(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com", UserRole.User);
        var request = new UserRequest { Name = "Alice Updated", Email = "alice@example.com", Role = UserRole.Admin };

        var updated = await service.UpdateAsync(user.Id, request);

        Assert.NotNull(updated);
        Assert.Equal("Alice Updated", updated.Name);
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var service = CreateService(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com", UserRole.User);

        var result = await service.ArchiveAsync(user.Id);
        var archived = await context.Users.FindAsync(user.Id);

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
        var user = await AddUserAsync(context, "Alice", "alice@example.com", UserRole.User, isArchived: true);

        var result = await service.UnarchiveAsync(user.Id);
        var unarchived = await context.Users.FindAsync(user.Id);

        Assert.True(result);
        Assert.False(unarchived!.IsArchived);
    }
}
