using System.Text.Json.Serialization;
using Microsoft.OpenApi;

namespace Api.Common.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddTszJson(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        return services;
    }

    public static IServiceCollection AddTszOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
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
        return services;
    }
}
