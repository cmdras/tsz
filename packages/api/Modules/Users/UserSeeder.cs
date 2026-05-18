using Api.Common.Database;
using Api.Modules.UserLeaveAllowances;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Users;

public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!await dbContext.Users.AnyAsync())
        {
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

        if (!await dbContext.UserLeaveAllowances.AnyAsync())
        {
            var currentYear = DateTime.UtcNow.Year;
            var users = await dbContext.Users.ToListAsync();
            var activeLeaveTypes = await dbContext.LeaveTypes
                .Where(leaveType => !leaveType.IsArchived)
                .ToListAsync();

            foreach (var user in users)
            {
                foreach (var leaveType in activeLeaveTypes)
                {
                    dbContext.UserLeaveAllowances.Add(new UserLeaveAllowance
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        LeaveTypeId = leaveType.Id,
                        Year = currentYear,
                        Mode = leaveType.DefaultMode,
                        TotalDays = leaveType.DefaultDays,
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
