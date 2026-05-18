using Api.Common.Database;
using Api.Modules.UserLeaveAllowances;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.LeaveTypes;

public static class LeaveTypeSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.LeaveTypes.AnyAsync())
            return;

        dbContext.LeaveTypes.AddRange(
            new LeaveType { Id = Guid.NewGuid(), Name = "Holiday",             DefaultDays = 20m, DefaultMode = AllowanceMode.Limited   },
            new LeaveType { Id = Guid.NewGuid(), Name = "ADV",                 DefaultDays = 5m,  DefaultMode = AllowanceMode.Limited   },
            new LeaveType { Id = Guid.NewGuid(), Name = "Sickness",            DefaultDays = 0m,  DefaultMode = AllowanceMode.Unlimited },
            new LeaveType { Id = Guid.NewGuid(), Name = "Ancienniteit",        DefaultDays = 0m,  DefaultMode = AllowanceMode.Limited   },
            new LeaveType { Id = Guid.NewGuid(), Name = "Holiday replacement", DefaultDays = 0m,  DefaultMode = AllowanceMode.Limited   }
        );
        await dbContext.SaveChangesAsync();
    }
}
