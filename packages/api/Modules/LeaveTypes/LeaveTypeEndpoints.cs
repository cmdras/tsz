using Api.Common;
using Api.Common.Extensions;
using Api.Common.Filters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Modules.LeaveTypes;

public static class LeaveTypeEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("leave-types");

        group.MapGet("/", async (
            string? search,
            LeaveTypeSort? sort,
            SortDirection? sortDirection,
            int? page,
            int? pageSize,
            bool? showArchived,
            LeaveTypeService service,
            CancellationToken cancellationToken) =>
        {
            var pageNumber = page is > 0 ? page.Value : 1;
            var size = pageSize is > 0 and <= 100 ? pageSize.Value : 25;
            return TypedResults.Ok(await service.GetAllAsync(
                search,
                sort ?? LeaveTypeSort.Name,
                sortDirection ?? SortDirection.Asc,
                pageNumber,
                size,
                showArchived ?? false,
                cancellationToken));
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<LeaveTypeResponse>, NotFound>> (
            Guid id,
            LeaveTypeService service,
            CancellationToken cancellationToken) =>
        {
            var leaveType = await service.GetByIdAsync(id, cancellationToken);
            return leaveType is not null
                ? TypedResults.Ok(leaveType)
                : TypedResults.NotFound();
        }).WithName("GetLeaveTypeById");

        group.MapPost("/", async Task<CreatedAtRoute<LeaveTypeResponse>> (
            LeaveTypeRequest request,
            LeaveTypeService service,
            CancellationToken cancellationToken) =>
        {
            var leaveType = await service.CreateAsync(request, cancellationToken);
            return TypedResults.CreatedAtRoute(leaveType, "GetLeaveTypeById", new { id = leaveType.Id });
        })
            .AddEndpointFilter<ValidationFilter<LeaveTypeRequest>>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async Task<Results<Ok<LeaveTypeResponse>, NotFound>> (
            Guid id,
            LeaveTypeRequest request,
            LeaveTypeService service,
            CancellationToken cancellationToken) =>
        {
            var leaveType = await service.UpdateAsync(id, request, cancellationToken);
            return leaveType is not null
                ? TypedResults.Ok(leaveType)
                : TypedResults.NotFound();
        })
            .AddEndpointFilter<ValidationFilter<LeaveTypeRequest>>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}/archive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            LeaveTypeService service,
            CancellationToken cancellationToken) =>
        {
            return await service.ArchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });

        group.MapPatch("/{id:guid}/unarchive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            LeaveTypeService service,
            CancellationToken cancellationToken) =>
        {
            return await service.UnarchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
