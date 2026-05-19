using Api.Common.Database;

namespace Api.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<UserService>();
        return services;
    }

    public static WebApplication MapUsersModule(this WebApplication app)
    {
        UserEndpoints.Map(app);
        return app;
    }

    public static Task SeedAsync(AppDbContext dbContext) => UserSeeder.SeedAsync(dbContext);
}
