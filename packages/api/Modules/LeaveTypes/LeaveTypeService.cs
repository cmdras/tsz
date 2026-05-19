using Api.Common;
using Api.Common.Database;
using Api.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.LeaveTypes;

public class DuplicateLeaveTypeNameException(string name)
    : DomainException($"A leave type named '{name}' already exists.", 409);

public class LeaveTypeService(AppDbContext dbContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<PagedLeaveTypes> GetAllAsync(
        string? search,
        LeaveTypeSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        bool showArchived,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LeaveTypes.AsQueryable();

        if (!showArchived)
            query = query.Where(leaveType => !leaveType.IsArchived);

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

        var total = await query.CountAsync(cancellationToken);
        var entities = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedLeaveTypes(entities.Select(ToResponse).ToList(), total);
    }

    public async Task<LeaveTypeResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveType = await _dbContext.LeaveTypes.FindAsync([id], cancellationToken);
        return leaveType is null ? null : ToResponse(leaveType);
    }

    public async Task<LeaveTypeResponse> CreateAsync(LeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim().ToLower();
        var isDuplicate = await _dbContext.LeaveTypes
            .AnyAsync(leaveType => leaveType.Name.ToLower() == normalizedName, cancellationToken);
        if (isDuplicate)
            throw new DuplicateLeaveTypeNameException(request.Name);

        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            DefaultDays = request.DefaultDays,
            DefaultMode = request.DefaultMode,
        };

        await _dbContext.LeaveTypes.AddAsync(leaveType, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(leaveType);
    }

    public async Task<LeaveTypeResponse?> UpdateAsync(Guid id, LeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        var leaveType = await _dbContext.LeaveTypes.FindAsync([id], cancellationToken);
        if (leaveType is null) return null;

        var normalizedName = request.Name.Trim().ToLower();
        var isDuplicate = await _dbContext.LeaveTypes
            .AnyAsync(other => other.Id != id && other.Name.ToLower() == normalizedName, cancellationToken);
        if (isDuplicate)
            throw new DuplicateLeaveTypeNameException(request.Name);

        leaveType.Name = request.Name.Trim();
        leaveType.DefaultDays = request.DefaultDays;
        leaveType.DefaultMode = request.DefaultMode;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(leaveType);
    }

    private static LeaveTypeResponse ToResponse(LeaveType leaveType) => new(
        leaveType.Id,
        leaveType.Name,
        leaveType.DefaultDays,
        leaveType.DefaultMode,
        leaveType.IsArchived);

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveType = await _dbContext.LeaveTypes.FindAsync([id], cancellationToken);
        if (leaveType is null) return false;

        leaveType.IsArchived = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveType = await _dbContext.LeaveTypes.FindAsync([id], cancellationToken);
        if (leaveType is null) return false;

        leaveType.IsArchived = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
