using Api.Common;

namespace Api.Modules.Customers;

public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedCustomers> GetAllAsync(
        string? search,
        CustomerSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await _repository.GetAllAsync(search, sort, sortDirection, page, pageSize, includeArchived, cancellationToken);
        return new PagedCustomers(items.Select(CustomerResponse.FromEntity).ToList(), total);
    }

    public async Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : CustomerResponse.FromEntity(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.CreateAsync(request, cancellationToken);
        return CustomerResponse.FromEntity(customer);
    }

    public async Task<CustomerResponse?> UpdateAsync(Guid id, CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.UpdateAsync(id, request, cancellationToken);
        return customer is null ? null : CustomerResponse.FromEntity(customer);
    }

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ArchiveAsync(id, cancellationToken);
    }

    public async Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.UnarchiveAsync(id, cancellationToken);
    }
}
