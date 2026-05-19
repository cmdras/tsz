using Api.Common.Extensions;

namespace Api.Modules.Stats;

public static class StatsEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("stats");

        group.MapGet("/admin", async (
            StatsService service,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.Ok(await service.GetAdminStatsAsync(cancellationToken));
        });
    }
}
