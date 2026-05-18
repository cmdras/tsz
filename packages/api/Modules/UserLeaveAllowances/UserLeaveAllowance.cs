namespace Api.Modules.UserLeaveAllowances;

public enum AllowanceMode
{
    Unlimited,
    Limited,
}

public class UserLeaveAllowance
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int Year { get; set; }
    public AllowanceMode Mode { get; set; }
    public decimal TotalDays { get; set; }
}
