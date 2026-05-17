using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.LeaveTypes;

public static class LeaveTypeSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.LeaveTypes.AnyAsync())
            return;

        dbContext.LeaveTypes.AddRange(
            new LeaveType { Id = Guid.NewGuid(), Name = "Holiday",             DefaultDays = 20m },
            new LeaveType { Id = Guid.NewGuid(), Name = "ADV",                 DefaultDays = 5m  },
            new LeaveType { Id = Guid.NewGuid(), Name = "Sickness",            DefaultDays = 0m  },
            new LeaveType { Id = Guid.NewGuid(), Name = "Ancienniteit",        DefaultDays = 0m  },
            new LeaveType { Id = Guid.NewGuid(), Name = "Holiday replacement", DefaultDays = 0m  }
        );
        await dbContext.SaveChangesAsync();
    }
}
