using Api.Modules.UserLeaveAllowances;

namespace Api.Modules.LeaveTypes;

public class LeaveType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultDays { get; set; }
    public AllowanceMode DefaultMode { get; set; }
    public bool IsArchived { get; set; }
}
