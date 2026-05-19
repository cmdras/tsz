using System.Data;
using System.Linq.Expressions;
using Api.Common;
using Api.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Modules.Users;

public class UserRepository : IUserRepository
{
    private static readonly IReadOnlyDictionary<UserRole, string> SearchableRoleLabels = new Dictionary<UserRole, string>
    {
        [UserRole.Admin] = "admin",
        [UserRole.User] = "user",
        [UserRole.ClientManager] = "client manager",
    };

    private static readonly Expression<Func<User, int>> RoleSortKey =
        user => user.Role == UserRole.Admin ? 0 : user.Role == UserRole.ClientManager ? 1 : 2;

    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<User> Items, int Total)> GetAllAsync(
        string? search,
        UserSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.Where(user => !user.IsArchived);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            var matchingRoles = SearchableRoleLabels
                .Where(pair => pair.Value.Contains(term))
                .Select(pair => pair.Key)
                .ToList();
            query = query.Where(user =>
                user.Name.ToLower().Contains(term) ||
                user.Email.ToLower().Contains(term) ||
                matchingRoles.Contains(user.Role));
        }

        var isDescending = sortDirection == SortDirection.Desc;
        query = (sort, isDescending) switch
        {
            (UserSort.Email, true) => query.OrderByDescending(user => user.Email),
            (UserSort.Email, false) => query.OrderBy(user => user.Email),
            (UserSort.Role, true) => query.OrderByDescending(RoleSortKey),
            (UserSort.Role, false) => query.OrderBy(RoleSortKey),
            (_, true) => query.OrderByDescending(user => user.Name),
            (_, false) => query.OrderBy(user => user.Name),
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FindAsync([id], cancellationToken);
    }

    public async Task<User> CreateAsync(UserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateAsync(Guid id, UserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return null;

        user.Name = request.Name;
        user.Email = request.Email;
        user.Role = request.Role;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLower();
        var query = _dbContext.Users.Where(user => user.Email.ToLower() == normalizedEmail);
        if (excludeId.HasValue)
            query = query.Where(user => user.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLower();
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => SetArchivedAsync(id, true, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => SetArchivedAsync(id, false, cancellationToken);

    public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        => _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

    private async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        user.IsArchived = isArchived;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
