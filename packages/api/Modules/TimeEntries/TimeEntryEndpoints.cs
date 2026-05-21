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
    }

    private static Guid GetUserIdFromClaims(HttpContext httpContext)
    {
        var oid = httpContext.User.FindFirst("oid")?.Value
            ?? httpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        return Guid.TryParse(oid, out var userId) ? userId : Guid.Empty;
    }
}
