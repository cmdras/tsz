using Api.Common;

namespace Api.Modules.Contracts;

public interface IContractRepository
{
    Task<(IReadOnlyList<Contract> Items, int Total)> GetAllAsync(
        string? search,
        ContractSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        bool includeArchived,
        CancellationToken cancellationToken = default);

    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Contract> CreateAsync(ContractRequest request, CancellationToken cancellationToken = default);

    Task<Contract?> UpdateAsync(Guid id, ContractRequest request, CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
