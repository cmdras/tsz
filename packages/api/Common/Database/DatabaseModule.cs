using Api.Common.Counters;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Common.Database;

public static class DatabaseModule
{
    public const string ConnectionStringKey = "AppDb";

    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringKey)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringKey}' is not configured.");
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await CustomersModule.SeedAsync(dbContext);
        await LeaveTypesModule.SeedAsync(dbContext);
        await UsersModule.SeedAsync(dbContext);
        await ContractsModule.SeedAsync(dbContext);
        await CountersModule.SeedAsync(dbContext);
    }
}
