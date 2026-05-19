using Api.Tests.Integration.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Auth;

public class AuthMetadataTests : IClassFixture<CustomerApiFactory>
{
    private readonly CustomerApiFactory _factory;

    public AuthMetadataTests(CustomerApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Every_api_endpoint_requires_authorization()
    {
        var endpointDataSource = _factory.Services.GetRequiredService<EndpointDataSource>();
        var unprotectedEndpoints = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("api/") == true)
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAuthorizeData>() is null)
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        Assert.Empty(unprotectedEndpoints);
    }
}
