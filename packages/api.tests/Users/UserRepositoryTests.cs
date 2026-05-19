using Api.Common;
using Api.Common.Database;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Tests.Users;

public class UserRepositoryTests
{
    private static UserRepository CreateRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options);
        return new UserRepository(context);
    }

    private static async Task<User> AddUserAsync(
        AppDbContext context,
        string name,
        string email,
        UserRole role = UserRole.User,
        bool isArchived = false)
    {
        var user = new User { Id = Guid.NewGuid(), Name = name, Email = email, Role = role, IsArchived = isArchived };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static UserRequest BuildRequest(
        string name = "Alice",
        string email = "alice@example.com",
        UserRole role = UserRole.User) => new()
    {
        Name = name,
        Email = email,
        Role = role,
        Leaves = [],
    };

    [Fact]
    public async Task GetAll_ExcludesArchivedUsers()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Active", "active@example.com");
        await AddUserAsync(context, "Archived", "archived@example.com", isArchived: true);

        var (items, total) = await repository.GetAllAsync(null, UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Active", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesName()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Alice Smith", "alice@example.com");
        await AddUserAsync(context, "Bob Jones", "bob@example.com");

        var (items, total) = await repository.GetAllAsync("alice", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Alice Smith", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesEmail()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Alice", "alice@company.com");
        await AddUserAsync(context, "Bob", "bob@other.com");

        var (items, total) = await repository.GetAllAsync("company", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Alice", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesRole()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Admin User", "admin@example.com", UserRole.Admin);
        await AddUserAsync(context, "Regular", "regular@example.com");

        var (items, total) = await repository.GetAllAsync("admin", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal("Admin User", items[0].Name);
    }

    [Fact]
    public async Task GetAll_SearchMatchesRoleDisplayLabelWithSpace()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Mike", "mike@example.com", UserRole.ClientManager);
        await AddUserAsync(context, "Regular", "regular@example.com");

        var (items, total) = await repository.GetAllAsync("Client Manager", UserSort.Name, SortDirection.Asc, 1, 25);

        Assert.Equal(1, total);
        Assert.Equal(UserRole.ClientManager, items[0].Role);
    }

    [Fact]
    public async Task GetAll_SortByRole_UsesLogicalOrder()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Zara User", "zara@example.com", UserRole.User);
        await AddUserAsync(context, "Alice Admin", "alice@example.com", UserRole.Admin);
        await AddUserAsync(context, "Mike Manager", "mike@example.com", UserRole.ClientManager);

        var (items, _) = await repository.GetAllAsync(null, UserSort.Role, SortDirection.Asc, 1, 25);

        Assert.Equal(UserRole.Admin, items[0].Role);
        Assert.Equal(UserRole.ClientManager, items[1].Role);
        Assert.Equal(UserRole.User, items[2].Role);
    }

    [Fact]
    public async Task GetAll_SortByRoleDesc_ReversesLogicalOrder()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Zara User", "zara@example.com", UserRole.User);
        await AddUserAsync(context, "Alice Admin", "alice@example.com", UserRole.Admin);
        await AddUserAsync(context, "Mike Manager", "mike@example.com", UserRole.ClientManager);

        var (items, _) = await repository.GetAllAsync(null, UserSort.Role, SortDirection.Desc, 1, 25);

        Assert.Equal(UserRole.User, items[0].Role);
        Assert.Equal(UserRole.ClientManager, items[1].Role);
        Assert.Equal(UserRole.Admin, items[2].Role);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var repository = CreateRepository(out var context);
        for (var index = 1; index <= 5; index++)
            await AddUserAsync(context, $"User {index}", $"user{index}@example.com");

        var (items, total) = await repository.GetAllAsync(null, UserSort.Name, SortDirection.Asc, 2, 2);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsUser()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com", UserRole.Admin);

        var found = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(found);
        Assert.Equal("Alice", found.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var found = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesUser()
    {
        var repository = CreateRepository(out _);

        var user = await repository.CreateAsync(BuildRequest("Alice", "alice@example.com", UserRole.Admin));

        Assert.Equal("Alice", user.Name);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public async Task Update_ValidRequest_UpdatesUser()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com");

        var updated = await repository.UpdateAsync(user.Id, BuildRequest("Alice Updated", "alice2@example.com", UserRole.Admin));

        Assert.NotNull(updated);
        Assert.Equal("Alice Updated", updated.Name);
        Assert.Equal("alice2@example.com", updated.Email);
        Assert.Equal(UserRole.Admin, updated.Role);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var updated = await repository.UpdateAsync(Guid.NewGuid(), BuildRequest());

        Assert.Null(updated);
    }

    [Fact]
    public async Task ExistsByEmail_ExistingEmail_ReturnsTrue()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Alice", "alice@example.com");

        var exists = await repository.ExistsByEmailAsync("alice@example.com");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByEmail_UnknownEmail_ReturnsFalse()
    {
        var repository = CreateRepository(out _);

        var exists = await repository.ExistsByEmailAsync("nobody@example.com");

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByEmail_ExcludeId_ExcludesMatchingUser()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com");

        var exists = await repository.ExistsByEmailAsync("alice@example.com", excludeId: user.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task FindByEmail_ExistingEmail_ReturnsUser()
    {
        var repository = CreateRepository(out var context);
        await AddUserAsync(context, "Alice", "alice@example.com");

        var found = await repository.FindByEmailAsync("alice@example.com");

        Assert.NotNull(found);
        Assert.Equal("Alice", found.Name);
    }

    [Fact]
    public async Task FindByEmail_UnknownEmail_ReturnsNull()
    {
        var repository = CreateRepository(out _);

        var found = await repository.FindByEmailAsync("nobody@example.com");

        Assert.Null(found);
    }

    [Fact]
    public async Task Archive_ExistingId_SetsIsArchivedTrue()
    {
        var repository = CreateRepository(out var context);
        var user = await AddUserAsync(context, "Alice", "alice@example.com");

        var result = await repository.ArchiveAsync(user.Id);
        var archived = await context.Users.FindAsync(user.Id);

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
        var user = await AddUserAsync(context, "Alice", "alice@example.com", isArchived: true);

        var result = await repository.UnarchiveAsync(user.Id);
        var unarchived = await context.Users.FindAsync(user.Id);

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
