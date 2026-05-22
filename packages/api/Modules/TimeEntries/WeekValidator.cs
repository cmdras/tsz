using Api.Modules.Contracts;

namespace Api.Modules.TimeEntries;

public record WeekValidationResult(bool IsValid, string? ErrorMessage)
{
    public static readonly WeekValidationResult Valid = new(true, null);
    public static WeekValidationResult Failure(string message) => new(false, message);
}

public static class WeekValidator
{
    public static WeekValidationResult Validate(
        IEnumerable<WeekCell> cells,
        DateOnly weekStart,
        Guid userId,
        IEnumerable<Contract> contracts)
    {
        var cellList = cells.ToList();
        var weekEnd = weekStart.AddDays(6);

        var taskMap = contracts
            .SelectMany(contract => contract.Tasks.Select(task => (Task: task, Contract: contract)))
            .ToDictionary(pair => pair.Task.Id);

        foreach (var cell in cellList)
        {
            if (cell.ContractTaskId.HasValue && cell.LeaveTypeId.HasValue)
                return WeekValidationResult.Failure("A cell cannot have both ContractTaskId and LeaveTypeId set.");

            if (!cell.ContractTaskId.HasValue && !cell.LeaveTypeId.HasValue)
                return WeekValidationResult.Failure("A cell must have either ContractTaskId or LeaveTypeId set.");

            if (cell.Hours <= 0 || cell.Hours > 24)
                return WeekValidationResult.Failure($"Hours must be between 0 (exclusive) and 24 (inclusive). Got: {cell.Hours}.");

            if (cell.Hours % 0.5m != 0)
                return WeekValidationResult.Failure($"Hours must be a multiple of 0.5. Got: {cell.Hours}.");

            var dayOfWeek = cell.Date.DayOfWeek;
            if (dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return WeekValidationResult.Failure($"Time entries cannot be logged on weekends. Got: {cell.Date}.");

            if (cell.ContractTaskId.HasValue)
            {
                if (!taskMap.TryGetValue(cell.ContractTaskId.Value, out var pair))
                    return WeekValidationResult.Failure($"ContractTask {cell.ContractTaskId} not found for this user.");

                if (pair.Contract.ConsultantId != userId)
                    return WeekValidationResult.Failure($"ContractTask {cell.ContractTaskId} does not belong to the current user.");

                if (pair.Contract.StartDate > weekEnd || (pair.Contract.EndDate.HasValue && pair.Contract.EndDate < weekStart))
                    return WeekValidationResult.Failure($"Contract does not intersect week {weekStart}.");

                if (pair.Task.IsArchived)
                    return WeekValidationResult.Failure($"ContractTask {cell.ContractTaskId} is archived.");
            }
        }

        var dailySums = cellList
            .GroupBy(cell => cell.Date)
            .Select(group => group.Sum(cell => cell.Hours));

        if (dailySums.Any(sum => sum > 24))
            return WeekValidationResult.Failure("Daily total hours cannot exceed 24.");

        return WeekValidationResult.Valid;
    }
}
