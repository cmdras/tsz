using Microsoft.EntityFrameworkCore;

namespace Api.Common.Database;

public static class DbContextExtensions
{
    public static async Task<bool> SetArchivedAsync<TEntity>(
        this DbContext dbContext,
        Guid id,
        bool isArchived,
        CancellationToken cancellationToken = default)
        where TEntity : class, IArchivable
    {
        var entity = await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
        if (entity is null) return false;

        entity.IsArchived = isArchived;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
