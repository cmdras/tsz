using Api.Common.Database;

namespace Api.Common.Counters;

public static class CountersModule
{
    public static IServiceCollection AddCounters(this IServiceCollection services)
    {
        services.AddScoped<ICounterService, CounterService>();
        return services;
    }

    public static Task SeedAsync(AppDbContext dbContext) => CounterSeeder.SeedAsync(dbContext);
}
