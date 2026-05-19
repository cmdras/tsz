using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Api.Common.Exceptions;
using Api.Modules.LeaveTypes;

namespace Api.Modules.UserLeaveAllowances;

public class DuplicateUserLeaveAllowanceException()
    : DomainException("A leave allowance for this user, leave type, and year already exists.", 409);

public class UnknownLeaveTypeException(Guid leaveTypeId)
    : DomainException($"Leave type {leaveTypeId} does not exist.", 400);

public record UserLeaveAllowanceResponse(
    Guid Id,
    Guid LeaveTypeId,
    string Name,
    AllowanceMode Mode,
    int Year,
    decimal TotalDays,
    decimal Taken,
    decimal? Balance);

public class UserLeaveAllowanceRequest
{
    public Guid? Id { get; set; }

    [Required]
    public Guid LeaveTypeId { get; set; }

    [JsonRequired]
    public AllowanceMode Mode { get; set; }

    [Range(0, 365)]
    [MaxOneDecimalPlace]
    public decimal TotalDays { get; set; }
}
