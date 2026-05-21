using Api.Common.Database;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Common.Auth;

public class JitProvisioningMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, AppDbContext dbContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var oidValue = httpContext.User.FindFirst("oid")?.Value;

            if (Guid.TryParse(oidValue, out var userId))
            {
                var exists = await dbContext.Users.AnyAsync(user => user.Id == userId);

                if (!exists)
                {
                    var email = httpContext.User.FindFirst("preferred_username")?.Value ?? string.Empty;
                    var name = httpContext.User.FindFirst("name")?.Value ?? email;

                    dbContext.Users.Add(new User
                    {
                        Id = userId,
                        Email = email,
                        Name = name,
                        Role = UserRole.User,
                        IsArchived = false,
                    });

                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        // race condition: another request inserted the same user first
                    }
                }
            }
        }

        await next(httpContext);
    }
}
