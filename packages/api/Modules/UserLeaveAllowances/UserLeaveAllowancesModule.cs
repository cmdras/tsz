namespace Api.Modules.UserLeaveAllowances;

public static class UserLeaveAllowancesModule
{
    public static IServiceCollection AddUserLeaveAllowancesModule(this IServiceCollection services)
    {
        services.AddScoped<IUserLeaveAllowanceRepository, UserLeaveAllowanceRepository>();
        return services;
    }
}
