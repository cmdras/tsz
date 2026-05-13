using System.ComponentModel.DataAnnotations;

namespace Api.Modules.Customers;

public enum CustomerSort
{
    Number,
    Name,
    ContactName,
    City,
}

public enum SortDirection
{
    Asc,
    Desc,
}

public record PagedCustomers(IReadOnlyList<Customer> Items, int Total);

public class CustomerRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [StringLength(20)]
    public string Zip { get; set; } = string.Empty;

    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [StringLength(200)]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(254)]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;
}
