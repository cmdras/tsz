using Api.Modules.Customers;

namespace Api.Tests.Integration.Common.Builders;

public class CustomerBuilder
{
    private string _name = "Test Customer";
    private bool _isArchived;

    public static CustomerBuilder Globex => new CustomerBuilder().Named("Globex Corp");
    public static CustomerBuilder Acme => new CustomerBuilder().Named("Acme");

    public CustomerBuilder Named(string name)
    {
        _name = name;
        return this;
    }

    public CustomerBuilder Archived()
    {
        _isArchived = true;
        return this;
    }

    public bool IsArchived => _isArchived;

    public CustomerRequest Build() => new()
    {
        Name = _name,
        Street = "Main 1",
        Zip = "1000",
        City = "Brussels",
        Country = "Belgium",
        ContactName = "Contact",
        ContactEmail = "contact@example.com",
    };
}
