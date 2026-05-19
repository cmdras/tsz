using Api.Common.Database;

namespace Api.Modules.LeaveTypes;

public static class LeaveTypesModule
{
    public static IServiceCollection AddLeaveTypesModule(this IServiceCollection services)
    {
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<LeaveTypeService>();
        return services;
    }

    public static WebApplication MapLeaveTypesModule(this WebApplication app)
    {
        LeaveTypeEndpoints.Map(app);
        return app;
    }

    public static Task SeedAsync(AppDbContext dbContext) => LeaveTypeSeeder.SeedAsync(dbContext);
}
