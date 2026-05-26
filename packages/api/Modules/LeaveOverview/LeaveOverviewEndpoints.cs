using Api.Common.Extensions;

namespace Api.Modules.LeaveOverview;

public static class LeaveOverviewEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("leave-overview");

        group.MapGet("/", async (
            int year,
            HttpContext httpContext,
            LeaveOverviewService service,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserIdFromClaims(httpContext);
            return TypedResults.Ok(await service.GetOverviewAsync(userId, year, cancellationToken));
        });
    }

    private static Guid GetUserIdFromClaims(HttpContext httpContext)
    {
        var oid = httpContext.User.FindFirst("oid")?.Value
            ?? httpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        return Guid.TryParse(oid, out var userId) ? userId : Guid.Empty;
    }
}
