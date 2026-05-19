using Api.Common;
using Api.Common.Counters;
using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Customers;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _dbContext;

    public CustomerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<Customer> Items, int Total)> GetAllAsync(
        string? search,
        CustomerSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers.Where(customer => !customer.IsArchived);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(customer =>
                customer.Name.ToLower().Contains(term) ||
                customer.ContactName.ToLower().Contains(term));
        }

        var isDescending = sortDirection == SortDirection.Desc;
        query = sort switch
        {
            CustomerSort.Name => isDescending ? query.OrderByDescending(customer => customer.Name) : query.OrderBy(customer => customer.Name),
            CustomerSort.ContactName => isDescending ? query.OrderByDescending(customer => customer.ContactName) : query.OrderBy(customer => customer.ContactName),
            CustomerSort.City => isDescending ? query.OrderByDescending(customer => customer.City) : query.OrderBy(customer => customer.City),
            _ => isDescending ? query.OrderByDescending(customer => customer.Number) : query.OrderBy(customer => customer.Number),
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers.FindAsync([id], cancellationToken);
    }

    public async Task<Customer> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var counter = await _dbContext.Counters.FindAsync([CounterKeys.Customer], cancellationToken)
            ?? throw new InvalidOperationException($"Counter '{CounterKeys.Customer}' not found.");

        counter.Value++;

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Number = counter.Value,
            Name = request.Name,
            Street = request.Street,
            Zip = request.Zip,
            City = request.City,
            Country = request.Country,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<Customer?> UpdateAsync(Guid id, CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers.FindAsync([id], cancellationToken);
        if (customer is null) return null;

        customer.Name = request.Name;
        customer.Street = request.Street;
        customer.Zip = request.Zip;
        customer.City = request.City;
        customer.Country = request.Country;
        customer.ContactName = request.ContactName;
        customer.ContactEmail = request.ContactEmail;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => SetArchivedAsync(id, true, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => SetArchivedAsync(id, false, cancellationToken);

    private async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FindAsync([id], cancellationToken);
        if (customer is null) return false;

        customer.IsArchived = isArchived;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
