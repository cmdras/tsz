using Api.Common.Exceptions;
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
        })
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static Guid GetUserIdFromClaims(HttpContext httpContext)
    {
        var oid = httpContext.User.FindFirst("oid")?.Value
            ?? httpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        if (!Guid.TryParse(oid, out var userId))
            throw new MissingIdentityClaimException();

        return userId;
    }

    private sealed class MissingIdentityClaimException() : DomainException("Required identity claim is missing.", 401);
}
