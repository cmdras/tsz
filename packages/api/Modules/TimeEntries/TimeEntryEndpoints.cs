using Api.Common.Extensions;

namespace Api.Modules.TimeEntries;

public static class TimeEntryEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("time-entries");

        group.MapGet("/weeks/{weekStart}", async (
            DateOnly weekStart,
            HttpContext httpContext,
            TimeEntryService service,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserIdFromClaims(httpContext);
            return TypedResults.Ok(await service.GetWeekAsync(userId, weekStart, cancellationToken));
        });

        group.MapGet("/pickers", async (
            DateOnly weekStart,
            HttpContext httpContext,
            TimeEntryService service,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserIdFromClaims(httpContext);
            return TypedResults.Ok(await service.GetPickerOptionsAsync(userId, weekStart, cancellationToken));
        });

        group.MapPut("/weeks/{weekStart}", async (
            DateOnly weekStart,
            UpdateWeekRequest request,
            HttpContext httpContext,
            TimeEntryService service,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserIdFromClaims(httpContext);
            return TypedResults.Ok(await service.UpdateWeekAsync(userId, weekStart, request, cancellationToken));
        })
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/months/{yearMonth}", async (
            string yearMonth,
            HttpContext httpContext,
            TimeEntryService service,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserIdFromClaims(httpContext);
            return TypedResults.Ok(await service.GetMonthAsync(userId, yearMonth, cancellationToken));
        });

        group.MapPost("/weeks/{weekStart}/submit", async (
            DateOnly weekStart,
            UpdateWeekRequest request,
            HttpContext httpContext,
            TimeEntryService service,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserIdFromClaims(httpContext);
            return TypedResults.Ok(await service.SubmitWeekAsync(userId, weekStart, request, cancellationToken));
        })
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static Guid GetUserIdFromClaims(HttpContext httpContext)
    {
        var oid = httpContext.User.FindFirst("oid")?.Value
            ?? httpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        return Guid.TryParse(oid, out var userId) ? userId : Guid.Empty;
    }
}
