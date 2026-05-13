using System.Reflection;
using System.Text.Json.Serialization;
using Api.Common.Database;
using Api.Modules.Animals;
using Api.Modules.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<AnimalDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=animals.db");
});
builder.Services.AddScoped<AnimalService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDb")
           ?? "Data Source=tsz.db");
});
builder.Services.AddScoped<CustomerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var animalDb = scope.ServiceProvider.GetRequiredService<AnimalDbContext>();
    animalDb.Database.EnsureCreated();
    await AnimalSeeder.SeedAsync(animalDb);

    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDb.Database.MigrateAsync();
    await CustomerSeeder.SeedAsync(appDb);
}

// app.UseHttpsRedirection();
app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference("/openapi", options =>
{
    options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
});

app.MapGet("/", () => new
{
    name = "Animal API",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
});

AnimalEndpoints.Map(app);
CustomerEndpoints.Map(app);

app.Run();
