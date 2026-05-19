namespace Api.Modules.Stats;

public static class StatsModule
{
    public static IServiceCollection AddStatsModule(this IServiceCollection services)
    {
        services.AddScoped<StatsService>();
        return services;
    }

    public static WebApplication MapStatsModule(this WebApplication app)
    {
        StatsEndpoints.Map(app);
        return app;
    }
}
