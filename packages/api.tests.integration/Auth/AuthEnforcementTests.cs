using Api.Tests.Integration.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Auth;

public class AuthEnforcementShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>
{
    [Fact]
    public void Require_Authorization_On_All_Endpoints()
    {
        var endpointDataSource = factory.Services.GetRequiredService<EndpointDataSource>();
        var unprotectedEndpoints = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("api/") == true)
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAuthorizeData>() is null)
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        Assert.Empty(unprotectedEndpoints);
    }
}
