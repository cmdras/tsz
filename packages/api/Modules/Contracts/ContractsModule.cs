using Api.Common.Database;

namespace Api.Modules.Contracts;

public static class ContractsModule
{
    public static IServiceCollection AddContractsModule(this IServiceCollection services)
    {
        services.AddScoped<ContractService>();
        return services;
    }

    public static WebApplication MapContractsModule(this WebApplication app)
    {
        ContractEndpoints.Map(app);
        return app;
    }

    public static Task SeedAsync(AppDbContext dbContext) => ContractSeeder.SeedAsync(dbContext);
}
