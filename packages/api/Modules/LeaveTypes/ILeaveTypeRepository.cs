using Api.Common;

namespace Api.Modules.LeaveTypes;

public interface ILeaveTypeRepository
{
    Task<(IReadOnlyList<LeaveType> Items, int Total)> GetAllAsync(
        string? search,
        LeaveTypeSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        ArchivedFilter archivedFilter = ArchivedFilter.Active,
        CancellationToken cancellationToken = default);

    Task<LeaveType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveType>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveType>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<LeaveType> CreateAsync(LeaveTypeRequest request, CancellationToken cancellationToken = default);

    Task<LeaveType?> UpdateAsync(Guid id, LeaveTypeRequest request, CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
