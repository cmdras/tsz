using Api.Common;
using Api.Common.Extensions;
using Api.Common.Filters;
using Api.Modules.UserLeaveAllowances;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Modules.Users;

public static class UserEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("users");

        group.MapGet("/me", async Task<Results<Ok<User>, ProblemHttpResult>> (
            HttpContext httpContext,
            UserService service,
            CancellationToken cancellationToken) =>
        {
            var name = httpContext.User.FindFirst("name")?.Value ?? "";
            var email = httpContext.User.FindFirst("preferred_username")?.Value
                ?? httpContext.User.FindFirst("upn")?.Value
                ?? httpContext.User.FindFirst("email")?.Value
                ?? "";

            if (string.IsNullOrEmpty(email))
                return TypedResults.Problem("No UPN claim in token.", statusCode: 400);

            var user = await service.GetOrProvisionAsync(name, email, cancellationToken);
            return TypedResults.Ok(user);
        });

        group.MapGet("/", async (
            string? search,
            UserSort? sort,
            SortDirection? sortDirection,
            int? page,
            int? pageSize,
            UserService service,
            CancellationToken cancellationToken) =>
        {
            var pageNumber = page is > 0 ? page.Value : 1;
            var size = pageSize is > 0 and <= 100 ? pageSize.Value : 25;
            return TypedResults.Ok(await service.GetAllAsync(
                search,
                sort ?? UserSort.Name,
                sortDirection ?? SortDirection.Asc,
                pageNumber,
                size,
                cancellationToken));
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<UserResponse>, NotFound>> (
            Guid id,
            UserService service,
            CancellationToken cancellationToken) =>
        {
            var user = await service.GetByIdAsync(id, cancellationToken);
            return user is not null
                ? TypedResults.Ok(user)
                : TypedResults.NotFound();
        }).WithName("GetUserById");

        group.MapPost("/", async Task<CreatedAtRoute<User>> (
            UserRequest request,
            UserService service,
            CancellationToken cancellationToken) =>
        {
            var user = await service.CreateAsync(request, cancellationToken);
            return TypedResults.CreatedAtRoute(user, "GetUserById", new { id = user.Id });
        })
            .AddEndpointFilter<ValidationFilter<UserRequest>>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async Task<Results<Ok<User>, NotFound>> (
            Guid id,
            UserRequest request,
            UserService service,
            CancellationToken cancellationToken) =>
        {
            var user = await service.UpdateAsync(id, request, cancellationToken);
            return user is not null
                ? TypedResults.Ok(user)
                : TypedResults.NotFound();
        })
            .AddEndpointFilter<ValidationFilter<UserRequest>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}/archive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            UserService service,
            CancellationToken cancellationToken) =>
        {
            return await service.ArchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });

        group.MapPatch("/{id:guid}/unarchive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            UserService service,
            CancellationToken cancellationToken) =>
        {
            return await service.UnarchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
