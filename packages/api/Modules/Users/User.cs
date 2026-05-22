using Api.Common;

namespace Api.Modules.Users;

public enum UserRole
{
    Admin,
    User,
    ClientManager,
}

public class User : IArchivable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsArchived { get; set; }
}
