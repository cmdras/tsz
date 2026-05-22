using Api.Common;
using Api.Modules.Users;

namespace Api.Modules.Contracts;

public class Contract : IArchivable
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public Guid CustomerId { get; set; }
    public Customers.Customer Customer { get; set; } = null!;
    public Guid ConsultantId { get; set; }
    public User Consultant { get; set; } = null!;
    public string Subject { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsArchived { get; set; }
    public ICollection<ContractTask> Tasks { get; set; } = [];
}
