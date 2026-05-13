using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Users;

public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Users.AnyAsync())
            return;

        dbContext.Users.AddRange(
            new User { Id = Guid.NewGuid(), Name = "Alice Admin",          Email = "alice.admin@tsz.be",          Role = UserRole.Admin },
            new User { Id = Guid.NewGuid(), Name = "Charlie Manager",      Email = "charlie.manager@tsz.be",      Role = UserRole.ClientManager },
            new User { Id = Guid.NewGuid(), Name = "Diana Manager",        Email = "diana.manager@tsz.be",        Role = UserRole.ClientManager },
            new User { Id = Guid.NewGuid(), Name = "Bob User",             Email = "bob.user@tsz.be",             Role = UserRole.User },
            new User { Id = Guid.NewGuid(), Name = "Eve User",             Email = "eve.user@tsz.be",             Role = UserRole.User },
            new User { Id = Guid.NewGuid(), Name = "Frank User",           Email = "frank.user@tsz.be",           Role = UserRole.User },
            new User { Id = Guid.NewGuid(), Name = "Grace User",           Email = "grace.user@tsz.be",           Role = UserRole.User },
            new User { Id = Guid.NewGuid(), Name = "Henry User",           Email = "henry.user@tsz.be",           Role = UserRole.User },
            new User { Id = Guid.NewGuid(), Name = "Irene User",           Email = "irene.user@tsz.be",           Role = UserRole.User },
            new User { Id = Guid.NewGuid(), Name = "Jack User",            Email = "jack.user@tsz.be",            Role = UserRole.User }
        );
        await dbContext.SaveChangesAsync();
    }
}
