using Api.Modules.Users;

namespace Api.Modules.TimeEntries;

public class WeekSubmission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly WeekStart { get; set; }
    public DateTime SubmittedAt { get; set; }
}
