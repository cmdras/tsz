namespace Api.Modules.Contracts;

public class ContractTask
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal DayRate { get; set; }
    public int Order { get; set; }
    public bool IsArchived { get; set; }
}
