using Api.Common;
using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.LeaveTypes;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly AppDbContext _dbContext;

    public LeaveTypeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<LeaveType> Items, int Total)> GetAllAsync(
        string? search,
        LeaveTypeSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        ArchivedFilter archivedFilter = ArchivedFilter.Active,
        CancellationToken cancellationToken = default)
    {
        var query = archivedFilter switch
        {
            ArchivedFilter.All => _dbContext.LeaveTypes.AsQueryable(),
            ArchivedFilter.Archived => _dbContext.LeaveTypes.Where(leaveType => leaveType.IsArchived),
            _ => _dbContext.LeaveTypes.Where(leaveType => !leaveType.IsArchived),
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(leaveType => leaveType.Name.ToLower().Contains(term));
        }

        var isDescending = sortDirection == SortDirection.Desc;
        query = sort switch
        {
            LeaveTypeSort.DefaultDays => isDescending
                ? query.OrderByDescending(leaveType => leaveType.DefaultDays)
                : query.OrderBy(leaveType => leaveType.DefaultDays),
            _ => isDescending
                ? query.OrderByDescending(leaveType => leaveType.Name)
                : query.OrderBy(leaveType => leaveType.Name),
        };

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<LeaveType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LeaveTypes.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveType>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LeaveTypes
            .Where(leaveType => ids.Contains(leaveType.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.LeaveTypes
            .Where(leaveType => !leaveType.IsArchived)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();
        var query = _dbContext.LeaveTypes.Where(leaveType => leaveType.Name.ToLower() == normalizedName);
        if (excludeId.HasValue)
            query = query.Where(leaveType => leaveType.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<LeaveType> CreateAsync(LeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            DefaultDays = request.DefaultDays,
            DefaultMode = request.DefaultMode,
        };

        _dbContext.LeaveTypes.Add(leaveType);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return leaveType;
    }

    public async Task<LeaveType?> UpdateAsync(Guid id, LeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        var leaveType = await _dbContext.LeaveTypes.FindAsync([id], cancellationToken);
        if (leaveType is null) return null;

        leaveType.Name = request.Name.Trim();
        leaveType.DefaultDays = request.DefaultDays;
        leaveType.DefaultMode = request.DefaultMode;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return leaveType;
    }

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.SetArchivedAsync<LeaveType>(id, true, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.SetArchivedAsync<LeaveType>(id, false, cancellationToken);
}
