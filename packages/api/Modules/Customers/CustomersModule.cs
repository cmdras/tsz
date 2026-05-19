using Api.Common.Database;

namespace Api.Modules.Customers;

public static class CustomersModule
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<CustomerService>();
        return services;
    }

    public static WebApplication MapCustomersModule(this WebApplication app)
    {
        CustomerEndpoints.Map(app);
        return app;
    }

    public static Task SeedAsync(AppDbContext dbContext) => CustomerSeeder.SeedAsync(dbContext);
}
