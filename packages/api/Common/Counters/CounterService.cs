using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Common.Counters;

public class CounterService : ICounterService
{
    private readonly AppDbContext _dbContext;

    public CounterService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> NextAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var counter = await _dbContext.Counters.FindAsync([key], cancellationToken);
        if (counter is null)
            throw new InvalidOperationException($"Counter '{key}' not found.");

        counter.Value++;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return counter.Value;
    }
}
