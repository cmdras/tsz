namespace Api.Modules.TimeEntries;

public static class TimeEntriesModule
{
    public static IServiceCollection AddTimeEntriesModule(this IServiceCollection services)
    {
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<TimeEntryService>();
        return services;
    }

    public static WebApplication MapTimeEntriesModule(this WebApplication app)
    {
        TimeEntryEndpoints.Map(app);
        return app;
    }
}
