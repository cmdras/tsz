using System.Data;
using Api.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Modules.Users;

public interface IUserRepository
{
    Task<(IReadOnlyList<User> Items, int Total)> GetAllAsync(
        string? search,
        UserSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User> CreateAsync(UserRequest request, CancellationToken cancellationToken = default);

    Task<User?> UpdateAsync(Guid id, UserRequest request, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
}
