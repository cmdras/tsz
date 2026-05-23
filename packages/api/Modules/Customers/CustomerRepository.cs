using System.Data;
using Api.Common;
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
        ArchivedFilter archivedFilter = ArchivedFilter.Active,
        CancellationToken cancellationToken = default)
    {
        var query = archivedFilter switch
        {
            ArchivedFilter.All => _dbContext.Customers.AsQueryable(),
            ArchivedFilter.Archived => _dbContext.Customers.Where(customer => customer.IsArchived),
            _ => _dbContext.Customers.Where(customer => !customer.IsArchived),
        };

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

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers.FindAsync([id], cancellationToken);
    }

    public async Task<Customer> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var nextNumber = (await _dbContext.Customers.MaxAsync(customer => (int?)customer.Number, cancellationToken) ?? 0) + 1;

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

        _dbContext.Customers.Add(customer);
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

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.SetArchivedAsync<Customer>(id, true, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.SetArchivedAsync<Customer>(id, false, cancellationToken);
}
