using System.Linq.Expressions;
using Api.Common;
using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Users;

public class UserService
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

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedUsers> GetAllAsync(
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
        return new PagedUsers(items, total);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Users.FindAsync([id], cancellationToken).AsTask();

    public async Task<User> CreateAsync(UserRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _dbContext.Users
            .AnyAsync(user => user.Email == request.Email, cancellationToken);
        if (emailExists)
            throw new DuplicateEmailException();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
        };

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateAsync(Guid id, UserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return null;

        var emailExists = await _dbContext.Users
            .AnyAsync(otherUser => otherUser.Email == request.Email && otherUser.Id != id, cancellationToken);
        if (emailExists)
            throw new DuplicateEmailException();

        user.Name = request.Name;
        user.Email = request.Email;
        user.Role = request.Role;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        user.IsArchived = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        user.IsArchived = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
