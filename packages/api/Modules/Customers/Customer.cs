using Api.Common;

namespace Api.Modules.Customers;

public class Customer : IArchivable
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
}
