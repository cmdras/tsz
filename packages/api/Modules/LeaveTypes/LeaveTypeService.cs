using Api.Common;
using Api.Common.Exceptions;

namespace Api.Modules.LeaveTypes;

public class DuplicateLeaveTypeNameException(string name)
    : DomainException($"A leave type named '{name}' already exists.", 409);

public class LeaveTypeService(ILeaveTypeRepository leaveTypeRepository)
{
    private readonly ILeaveTypeRepository _leaveTypeRepository = leaveTypeRepository;

    public async Task<PagedLeaveTypes> GetAllAsync(
        string? search,
        LeaveTypeSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        ArchivedFilter archivedFilter = ArchivedFilter.Active,
        CancellationToken cancellationToken = default)
    {
        var (entities, total) = await _leaveTypeRepository.GetAllAsync(search, sort, sortDirection, page, pageSize, archivedFilter, cancellationToken);
        return new PagedLeaveTypes(entities.Select(ToResponse).ToList(), total);
    }

    public async Task<LeaveTypeResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(id, cancellationToken);
        return leaveType is null ? null : ToResponse(leaveType);
    }

    public async Task<LeaveTypeResponse> CreateAsync(LeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (await _leaveTypeRepository.ExistsByNameAsync(request.Name, cancellationToken: cancellationToken))
            throw new DuplicateLeaveTypeNameException(request.Name);

        var leaveType = await _leaveTypeRepository.CreateAsync(request, cancellationToken);
        return ToResponse(leaveType);
    }

    public async Task<LeaveTypeResponse?> UpdateAsync(Guid id, LeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (await _leaveTypeRepository.GetByIdAsync(id, cancellationToken) is null)
            return null;

        if (await _leaveTypeRepository.ExistsByNameAsync(request.Name, excludeId: id, cancellationToken: cancellationToken))
            throw new DuplicateLeaveTypeNameException(request.Name);

        var leaveType = await _leaveTypeRepository.UpdateAsync(id, request, cancellationToken);
        return leaveType is null ? null : ToResponse(leaveType);
    }

    private static LeaveTypeResponse ToResponse(LeaveType leaveType) => new(
        leaveType.Id,
        leaveType.Name,
        leaveType.DefaultDays,
        leaveType.DefaultMode,
        leaveType.IsArchived);

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _leaveTypeRepository.ArchiveAsync(id, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _leaveTypeRepository.UnarchiveAsync(id, cancellationToken);
}
