using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.UserLeaveAllowances;

public class UserLeaveAllowanceRepository : IUserLeaveAllowanceRepository
{
    private readonly AppDbContext _dbContext;

    public UserLeaveAllowanceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserLeaveAllowance>> GetForUserAndYearAsync(
        Guid userId,
        int year,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserLeaveAllowances
            .Where(allowance => allowance.UserId == userId && allowance.Year == year)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyList<UserLeaveAllowance> entities, CancellationToken cancellationToken = default)
    {
        if (entities.Count == 0) return;
        _dbContext.UserLeaveAllowances.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var allowance = await _dbContext.UserLeaveAllowances.FindAsync([id], cancellationToken);
        if (allowance is null) return;

        _dbContext.UserLeaveAllowances.Remove(allowance);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRangeAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count > 0)
        {
            var allowances = await _dbContext.UserLeaveAllowances
                .Where(allowance => ids.Contains(allowance.Id))
                .ToListAsync(cancellationToken);
            _dbContext.UserLeaveAllowances.RemoveRange(allowances);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
