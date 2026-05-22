using Api.Modules.TimeEntries;

namespace Api.Tests.TimeEntries;

public class WeekDifferShould
{
    private static TimeEntry MakeEntry(Guid id, DateOnly date, Guid contractTaskId, decimal hours) =>
        new() { Id = id, UserId = Guid.NewGuid(), Date = date, ContractTaskId = contractTaskId, Hours = hours, UpdatedAt = DateTime.UtcNow };

    private static WeekCell MakeCell(DateOnly date, Guid contractTaskId, decimal hours) =>
        new(ContractTaskId: contractTaskId, LeaveTypeId: null, Date: date, Hours: hours);

    private static readonly DateOnly Monday = new(2026, 5, 18);

    // --- Tracer bullet: empty inputs → empty diff ---

    [Fact]
    public void Return_Empty_Diff_When_Both_Inputs_Are_Empty()
    {
        var (toUpsert, toDelete) = WeekDiffer.Diff([], []);

        Assert.Empty(toUpsert);
        Assert.Empty(toDelete);
    }

    // --- Identical state → empty diff ---

    [Fact]
    public void Return_Empty_Diff_When_Incoming_Matches_Existing()
    {
        var taskId = Guid.NewGuid();
        var existing = new[] { MakeEntry(Guid.NewGuid(), Monday, taskId, 8m) };
        var incoming = new[] { MakeCell(Monday, taskId, 8m) };

        var (toUpsert, toDelete) = WeekDiffer.Diff(existing, incoming);

        Assert.Empty(toUpsert);
        Assert.Empty(toDelete);
    }

    // --- New cells only → toUpsert, no deletes ---

    [Fact]
    public void Return_New_Cell_As_Upsert_When_No_Existing()
    {
        var taskId = Guid.NewGuid();
        var incoming = new[] { MakeCell(Monday, taskId, 8m) };

        var (toUpsert, toDelete) = WeekDiffer.Diff([], incoming);

        Assert.Single(toUpsert);
        Assert.Equal(taskId, toUpsert[0].ContractTaskId);
        Assert.Empty(toDelete);
    }

    // --- Removed cells → toDelete ---

    [Fact]
    public void Return_Existing_Id_As_Delete_When_Not_In_Incoming()
    {
        var entryId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var existing = new[] { MakeEntry(entryId, Monday, taskId, 8m) };

        var (toUpsert, toDelete) = WeekDiffer.Diff(existing, []);

        Assert.Empty(toUpsert);
        Assert.Single(toDelete);
        Assert.Equal(entryId, toDelete[0]);
    }

    // --- Changed hours → toUpsert ---

    [Fact]
    public void Return_Cell_As_Upsert_When_Hours_Changed()
    {
        var taskId = Guid.NewGuid();
        var existing = new[] { MakeEntry(Guid.NewGuid(), Monday, taskId, 8m) };
        var incoming = new[] { MakeCell(Monday, taskId, 4m) };

        var (toUpsert, toDelete) = WeekDiffer.Diff(existing, incoming);

        Assert.Single(toUpsert);
        Assert.Equal(4m, toUpsert[0].Hours);
        Assert.Empty(toDelete);
    }

    // --- Zero-hour cells treated as delete ---

    [Fact]
    public void Delete_Existing_Cell_When_Incoming_Hours_Are_Zero()
    {
        var entryId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var existing = new[] { MakeEntry(entryId, Monday, taskId, 8m) };
        var incoming = new[] { MakeCell(Monday, taskId, 0m) };

        var (toUpsert, toDelete) = WeekDiffer.Diff(existing, incoming);

        Assert.Empty(toUpsert);
        Assert.Single(toDelete);
        Assert.Equal(entryId, toDelete[0]);
    }

    [Fact]
    public void Not_Upsert_New_Cell_With_Zero_Hours()
    {
        var taskId = Guid.NewGuid();
        var incoming = new[] { MakeCell(Monday, taskId, 0m) };

        var (toUpsert, toDelete) = WeekDiffer.Diff([], incoming);

        Assert.Empty(toUpsert);
        Assert.Empty(toDelete);
    }

    // --- Mixed: some upsert, some delete ---

    [Fact]
    public void Handle_Mixed_Upsert_And_Delete()
    {
        var existingId = Guid.NewGuid();
        var existingTaskId = Guid.NewGuid();
        var newTaskId = Guid.NewGuid();

        var existing = new[] { MakeEntry(existingId, Monday, existingTaskId, 8m) };
        var incoming = new[] { MakeCell(Monday, newTaskId, 4m) };

        var (toUpsert, toDelete) = WeekDiffer.Diff(existing, incoming);

        Assert.Single(toUpsert);
        Assert.Equal(newTaskId, toUpsert[0].ContractTaskId);
        Assert.Single(toDelete);
        Assert.Equal(existingId, toDelete[0]);
    }

    // --- Unordered inputs → same result ---

    [Fact]
    public void Tolerate_Unordered_Inputs()
    {
        var taskA = Guid.NewGuid();
        var taskB = Guid.NewGuid();
        var tuesday = Monday.AddDays(1);

        var existing = new[]
        {
            MakeEntry(Guid.NewGuid(), tuesday, taskA, 8m),
            MakeEntry(Guid.NewGuid(), Monday, taskB, 4m),
        };
        var incoming = new[]
        {
            MakeCell(Monday, taskB, 4m),
            MakeCell(tuesday, taskA, 8m),
        };

        var (toUpsert, toDelete) = WeekDiffer.Diff(existing, incoming);

        Assert.Empty(toUpsert);
        Assert.Empty(toDelete);
    }
}
