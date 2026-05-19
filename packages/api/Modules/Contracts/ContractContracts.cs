using System.ComponentModel.DataAnnotations;

namespace Api.Modules.Contracts;

public enum ContractSort
{
    Number,
    Customer,
    Subject,
    Consultant,
    StartDate,
    EndDate,
}

public record ContractTaskResponse(
    Guid Id,
    string Name,
    decimal DayRate,
    int Order,
    bool IsArchived);

public record ContractResponse(
    Guid Id,
    int Number,
    Guid CustomerId,
    string CustomerName,
    Guid ConsultantId,
    string ConsultantName,
    string Subject,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsArchived,
    IReadOnlyList<ContractTaskResponse> Tasks);

public record PagedContracts(IReadOnlyList<ContractResponse> Items, int Total);

public class ContractTaskRequest
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Day rate must be greater than 0")]
    public decimal DayRate { get; set; }
}

public class ContractRequest
{
    public Guid CustomerId { get; set; }

    public Guid ConsultantId { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public List<ContractTaskRequest> Tasks { get; set; } = [];
}
