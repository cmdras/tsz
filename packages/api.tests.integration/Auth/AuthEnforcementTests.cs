using System.Net;
using Api.Tests.Integration.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Integration.Auth;

public class AuthEnforcementTests : IClassFixture<AuthEnforcedApiFactory>
{
    private readonly HttpClient _client;

    public AuthEnforcementTests(AuthEnforcedApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [Theory]
    [InlineData("/api/customers")]
    [InlineData("/api/users")]
    [InlineData("/api/contracts")]
    [InlineData("/api/leave-types")]
    public async Task Get_WithoutBearerToken_ReturnsUnauthorized(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class AuthEnforcedApiFactory : TestApiFactory
{
    public AuthEnforcedApiFactory() : base("AuthEnforcementTests") { }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:TenantId"] = "test-tenant-id",
                ["Auth:ClientId"] = "test-client-id",
                ["Auth:Disabled"] = "false",
            });
        });
    }
}
