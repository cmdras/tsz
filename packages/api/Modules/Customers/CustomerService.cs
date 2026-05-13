using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Customers;

public class CustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedCustomers> GetAllAsync(
        string? search,
        CustomerSort sort,
        SortDirection dir,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Customers.Where(c => !c.IsArchived);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.ContactName.ToLower().Contains(term));
        }

        var desc = dir == SortDirection.Desc;
        query = sort switch
        {
            CustomerSort.Name => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            CustomerSort.ContactName => desc ? query.OrderByDescending(c => c.ContactName) : query.OrderBy(c => c.ContactName),
            CustomerSort.City => desc ? query.OrderByDescending(c => c.City) : query.OrderBy(c => c.City),
            _ => desc ? query.OrderByDescending(c => c.Number) : query.OrderBy(c => c.Number),
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedCustomers(items, total);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Customers.FindAsync([id], ct).AsTask();

    public async Task<Customer> CreateAsync(CustomerRequest req, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        var nextNumber = (await _db.Customers.MaxAsync(c => (int?)c.Number, ct) ?? 99999) + 1;

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Number = nextNumber,
            Name = req.Name,
            Street = req.Street,
            Zip = req.Zip,
            City = req.City,
            Country = req.Country,
            ContactName = req.ContactName,
            ContactEmail = req.ContactEmail,
        };

        await _db.Customers.AddAsync(customer, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return customer;
    }

    public async Task<Customer?> UpdateAsync(Guid id, CustomerRequest req, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FindAsync([id], ct);
        if (customer is null) return null;

        customer.Name = req.Name;
        customer.Street = req.Street;
        customer.Zip = req.Zip;
        customer.City = req.City;
        customer.Country = req.Country;
        customer.ContactName = req.ContactName;
        customer.ContactEmail = req.ContactEmail;

        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FindAsync([id], ct);
        if (customer is null) return false;

        customer.IsArchived = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UnarchiveAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FindAsync([id], ct);
        if (customer is null) return false;

        customer.IsArchived = false;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
