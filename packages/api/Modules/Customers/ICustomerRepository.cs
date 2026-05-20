using Api.Common;

namespace Api.Modules.Customers;

public interface ICustomerRepository
{
    Task<(IReadOnlyList<Customer> Items, int Total)> GetAllAsync(
        string? search,
        CustomerSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        ArchivedFilter archivedFilter = ArchivedFilter.Active,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Customer> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default);

    Task<Customer?> UpdateAsync(Guid id, CustomerRequest request, CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
