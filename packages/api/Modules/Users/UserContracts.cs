using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Api.Modules.UserLeaveAllowances;

namespace Api.Modules.Users;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException() : base("Email address is already in use.") { }
}

public enum UserSort
{
    Name,
    Email,
    Role,
}

public record PagedUsers(IReadOnlyList<User> Items, int Total);

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    bool IsArchived,
    IReadOnlyList<UserLeaveAllowanceResponse> Leaves);

public class UserRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [JsonRequired]
    public UserRole Role { get; set; }

    [JsonRequired]
    public List<UserLeaveAllowanceRequest> Leaves { get; set; } = [];
}
