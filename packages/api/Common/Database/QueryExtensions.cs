using Microsoft.EntityFrameworkCore;

namespace Api.Common.Database;

public static class QueryExtensions
{
    public static async Task<(IReadOnlyList<T> Items, int Total)> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }
}
