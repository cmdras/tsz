using Api.Modules.Contracts;
using Api.Modules.LeaveTypes;
using Api.Modules.Users;

namespace Api.Modules.TimeEntries;

public class TimeEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly Date { get; set; }
    public Guid? ContractTaskId { get; set; }
    public ContractTask? ContractTask { get; set; }
    public Guid? LeaveTypeId { get; set; }
    public LeaveType? LeaveType { get; set; }
    public decimal Hours { get; set; }
    public DateTime UpdatedAt { get; set; }
}
