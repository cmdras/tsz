using System.Reflection;
using System.Text.Json.Serialization;
using Api.Common.Counters;
using Api.Common.Database;
using Api.Common.Extensions;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddEntraJwtAuth();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, _, _) =>
    {
        if (schema.Properties is { Count: > 0 })
        {
            schema.Required ??= new HashSet<string>();
            foreach (var (name, property) in schema.Properties)
            {
                var isNullable = property.Type is { } schemaType && (schemaType & JsonSchemaType.Null) != 0;
                if (!isNullable)
                {
                    schema.Required.Add(name);
                }
            }
        }
        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDb")
           ?? "Data Source=tsz.db");
});
builder.Services.AddScoped<ICounterService, CounterService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<LeaveTypeService>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDb.Database.MigrateAsync();
    await CustomerSeeder.SeedAsync(appDb);
    await LeaveTypeSeeder.SeedAsync(appDb);
    await UserSeeder.SeedAsync(appDb);
    await ContractSeeder.SeedAsync(appDb);
    await CounterSeeder.SeedAsync(appDb);
}

app.UseEntraJwtAuth();

// app.UseHttpsRedirection();
app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference("/openapi", options =>
{
    options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
});

app.MapGet("/", () => new
{
    name = "TSZ API",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
});

CustomerEndpoints.Map(app);
UserEndpoints.Map(app);
ContractEndpoints.Map(app);
LeaveTypeEndpoints.Map(app);

app.Run();
