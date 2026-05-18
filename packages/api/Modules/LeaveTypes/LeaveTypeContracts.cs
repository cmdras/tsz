using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Api.Modules.UserLeaveAllowances;

namespace Api.Modules.LeaveTypes;

public enum LeaveTypeSort
{
    Name,
    DefaultDays,
}

public record PagedLeaveTypes(IReadOnlyList<LeaveType> Items, int Total);

public class LeaveTypeRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 365)]
    [MaxOneDecimalPlace]
    public decimal DefaultDays { get; set; }

    [JsonRequired]
    public AllowanceMode DefaultMode { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxOneDecimalPlaceAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is not decimal decimalValue || decimalValue == Math.Round(decimalValue, 1);

    public override string FormatErrorMessage(string name) =>
        $"{name} must have at most one decimal place.";
}
