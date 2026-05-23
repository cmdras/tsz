using System.Data;
using Api.Common;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;

namespace Api.Modules.Users;

public class UserService(IUserRepository userRepository, IUserLeaveAllowanceRepository userLeaveAllowanceRepository, ILeaveTypeRepository leaveTypeRepository)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserLeaveAllowanceRepository _userLeaveAllowanceRepository = userLeaveAllowanceRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository = leaveTypeRepository;

    public async Task<PagedUsers> GetAllAsync(
        string? search,
        UserSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        ArchivedFilter archivedFilter = ArchivedFilter.Active,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await _userRepository.GetAllAsync(search, sort, sortDirection, page, pageSize, archivedFilter, cancellationToken);
        return new PagedUsers(items, total);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;

        var currentYear = DateTime.UtcNow.Year;
        var allowances = await _userLeaveAllowanceRepository.GetForUserAndYearAsync(id, currentYear, cancellationToken);
        var leaveTypeIds = allowances.Select(allowance => allowance.LeaveTypeId).ToList();
        var leaveTypesById = (await _leaveTypeRepository.GetByIdsAsync(leaveTypeIds, cancellationToken))
            .ToDictionary(leaveType => leaveType.Id);

        var leaves = allowances.Select(allowance => new UserLeaveAllowanceResponse(
            allowance.Id,
            allowance.LeaveTypeId,
            leaveTypesById.TryGetValue(allowance.LeaveTypeId, out var leaveType) ? leaveType.Name : string.Empty,
            allowance.Mode,
            allowance.Year,
            allowance.TotalDays,
            0m,
            allowance.Mode == AllowanceMode.Limited ? allowance.TotalDays : (decimal?)null)).ToList();

        return new UserResponse(user.Id, user.Name, user.Email, user.Role, user.IsArchived, leaves);
    }

    public async Task<User> CreateAsync(UserRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _userRepository.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken: cancellationToken))
            throw new DuplicateEmailException();

        var user = await _userRepository.CreateAsync(request, cancellationToken);
        await SeedLeaveAllowancesAsync(user, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateAsync(Guid id, UserRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _userRepository.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;

        if (await _userRepository.ExistsByEmailAsync(request.Email, excludeId: id, cancellationToken: cancellationToken))
            throw new DuplicateEmailException();

        var updatedUser = await _userRepository.UpdateAsync(id, request, cancellationToken);

        var currentYear = DateTime.UtcNow.Year;
        var existingAllowances = await _userLeaveAllowanceRepository.GetForUserAndYearAsync(id, currentYear, cancellationToken);
        var existingById = existingAllowances.ToDictionary(allowance => allowance.Id);

        var incomingLeaveTypeIds = request.Leaves
            .Where(leaf => !leaf.Id.HasValue)
            .Select(leaf => leaf.LeaveTypeId)
            .ToHashSet();
        if (incomingLeaveTypeIds.Count > 0)
        {
            var knownLeaveTypes = await _leaveTypeRepository.GetByIdsAsync(incomingLeaveTypeIds, cancellationToken);
            var knownLeaveTypeIds = knownLeaveTypes.Select(leaveType => leaveType.Id).ToHashSet();
            var unknownLeaveTypeId = incomingLeaveTypeIds.FirstOrDefault(leaveTypeId => !knownLeaveTypeIds.Contains(leaveTypeId));
            if (unknownLeaveTypeId != Guid.Empty)
                throw new UnknownLeaveTypeException(unknownLeaveTypeId);
        }

        var keptIds = new HashSet<Guid>();
        var occupiedLeaveTypeIds = new HashSet<Guid>();
        var newAllowances = new List<UserLeaveAllowance>();
        var updatedAllowances = new List<UserLeaveAllowance>();
        foreach (var leaf in request.Leaves)
        {
            if (leaf.Id.HasValue && existingById.TryGetValue(leaf.Id.Value, out var match))
            {
                keptIds.Add(match.Id);
                if (!occupiedLeaveTypeIds.Add(match.LeaveTypeId))
                    throw new DuplicateUserLeaveAllowanceException();
                if (match.Mode != leaf.Mode || match.TotalDays != leaf.TotalDays)
                {
                    match.Mode = leaf.Mode;
                    match.TotalDays = leaf.TotalDays;
                    updatedAllowances.Add(match);
                }
            }
            else if (!leaf.Id.HasValue)
            {
                if (!occupiedLeaveTypeIds.Add(leaf.LeaveTypeId))
                    throw new DuplicateUserLeaveAllowanceException();
                newAllowances.Add(new UserLeaveAllowance
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    LeaveTypeId = leaf.LeaveTypeId,
                    Year = currentYear,
                    Mode = leaf.Mode,
                    TotalDays = leaf.TotalDays,
                });
            }
        }
        await _userLeaveAllowanceRepository.UpdateRangeAsync(updatedAllowances, cancellationToken);
        await _userLeaveAllowanceRepository.AddRangeAsync(newAllowances, cancellationToken);

        var idsToRemove = existingAllowances
            .Where(allowance => !keptIds.Contains(allowance.Id))
            .Select(allowance => allowance.Id)
            .ToList();
        await _userLeaveAllowanceRepository.RemoveRangeAsync(idsToRemove, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return updatedUser;
    }

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _userRepository.ArchiveAsync(id, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _userRepository.UnarchiveAsync(id, cancellationToken);

    public async Task<User> GetOrProvisionAsync(string name, string email, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _userRepository.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var existing = await _userRepository.FindByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return existing;
        }

        var request = new UserRequest { Name = name, Email = email, Role = UserRole.User, Leaves = [] };
        var user = await _userRepository.CreateAsync(request, cancellationToken);
        await SeedLeaveAllowancesAsync(user, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return user;
    }

    private async Task SeedLeaveAllowancesAsync(User user, CancellationToken cancellationToken)
    {
        var currentYear = DateTime.UtcNow.Year;
        var activeLeaveTypes = await _leaveTypeRepository.GetActiveAsync(cancellationToken);

        var allowances = activeLeaveTypes.Select(leaveType => new UserLeaveAllowance
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LeaveTypeId = leaveType.Id,
            Year = currentYear,
            Mode = leaveType.DefaultMode,
            TotalDays = leaveType.DefaultDays,
        }).ToList();

        await _userLeaveAllowanceRepository.AddRangeAsync(allowances, cancellationToken);
    }
}
