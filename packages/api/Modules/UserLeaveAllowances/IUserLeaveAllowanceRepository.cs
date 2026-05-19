namespace Api.Modules.UserLeaveAllowances;

public interface IUserLeaveAllowanceRepository
{
    Task<IReadOnlyList<UserLeaveAllowance>> GetForUserAndYearAsync(
        Guid userId,
        int year,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(IReadOnlyList<UserLeaveAllowance> entities, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task RemoveRangeAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);
}
