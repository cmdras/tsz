using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Common.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapApiGroup(this IEndpointRouteBuilder endpoints, string prefix)
    {
        var group = endpoints.MapGroup($"/api/{prefix}").WithTags(prefix);
        var configuration = endpoints.ServiceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>(AuthExtensions.DisabledKey))
            group.RequireAuthorization();
        return group;
    }
}
