namespace Api.Modules.LeaveOverview;

public static class LeaveOverviewModule
{
    public static IServiceCollection AddLeaveOverviewModule(this IServiceCollection services)
    {
        services.AddScoped<LeaveOverviewService>();
        return services;
    }

    public static WebApplication MapLeaveOverviewModule(this WebApplication app)
    {
        LeaveOverviewEndpoints.Map(app);
        return app;
    }
}
