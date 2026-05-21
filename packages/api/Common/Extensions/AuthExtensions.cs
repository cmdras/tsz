using Api.Common.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace Api.Common.Extensions;

public static class AuthExtensions
{
    public static IApplicationBuilder UseJitProvisioning(this IApplicationBuilder app)
        => app.UseMiddleware<JitProvisioningMiddleware>();

    public static IServiceCollection AddTszAuthentication(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment)
    {
        services.AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.MapInboundClaims = false;
        });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
