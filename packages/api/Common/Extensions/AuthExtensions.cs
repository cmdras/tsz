using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Common.Extensions;

public static class AuthExtensions
{
    public const string DisabledKey = "Auth:Disabled";

    public static WebApplicationBuilder AddEntraJwtAuth(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetValue<bool>(DisabledKey)) return builder;

        var tenantId = builder.Configuration["Auth:TenantId"];
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new InvalidOperationException("Auth:TenantId is missing");
        var clientId = builder.Configuration["Auth:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Auth:ClientId is missing");

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = [$"api://{clientId}"],
                    ValidIssuers   = [$"https://login.microsoftonline.com/{tenantId}/v2.0"],
                };
            });
        builder.Services.AddAuthorization();
        return builder;
    }

    public static WebApplication UseEntraJwtAuth(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>(DisabledKey))
        {
            app.Logger.LogWarning("Auth:Disabled=true — API is unauthenticated. Do not run this in any shared env.");
            return app;
        }
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
