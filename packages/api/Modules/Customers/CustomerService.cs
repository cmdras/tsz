using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Customers;

public class CustomerService
{
    private readonly AppDbContext _dbContext;

    public CustomerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedCustomers> GetAllAsync(
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
        return new PagedCustomers(items, total);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Customers.FindAsync([id], cancellationToken).AsTask();

    public async Task<Customer> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var nextNumber = (await _dbContext.Customers.MaxAsync(customer => (int?)customer.Number, cancellationToken) ?? 99999) + 1;

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Number = nextNumber,
            Name = request.Name,
            Street = request.Street,
            Zip = request.Zip,
            City = request.City,
            Country = request.Country,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
        };

        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
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

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers.FindAsync([id], cancellationToken);
        if (customer is null) return false;

        customer.IsArchived = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers.FindAsync([id], cancellationToken);
        if (customer is null) return false;

        customer.IsArchived = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
