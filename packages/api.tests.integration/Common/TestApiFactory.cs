using Api.Common.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Common;

public abstract class TestApiFactory(string databaseName) : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = databaseName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Disabled"] = "true",
            });
        });
        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>) ||
                    descriptor.ServiceType == typeof(AppDbContext))
                .ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options
                .UseInMemoryDatabase(DatabaseName)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }
}
