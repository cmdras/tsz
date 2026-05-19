using Api.Common;
using Api.Common.Extensions;
using Api.Common.Filters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Modules.Customers;

public static class CustomerEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("customers");

        group.MapGet("/", async (
            string? search,
            CustomerSort? sort,
            SortDirection? sortDirection,
            int? page,
            int? pageSize,
            CustomerService service,
            CancellationToken cancellationToken) =>
        {
            var pageNumber = page is > 0 ? page.Value : 1;
            var size = pageSize is > 0 and <= 100 ? pageSize.Value : 25;
            return TypedResults.Ok(await service.GetAllAsync(
                search,
                sort ?? CustomerSort.Number,
                sortDirection ?? SortDirection.Asc,
                pageNumber,
                size,
                cancellationToken));
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<CustomerResponse>, NotFound>> (
            Guid id,
            CustomerService service,
            CancellationToken cancellationToken) =>
        {
            var customer = await service.GetByIdAsync(id, cancellationToken);
            return customer is not null
                ? TypedResults.Ok(customer)
                : TypedResults.NotFound();
        }).WithName("GetCustomerById");

        group.MapPost("/", async Task<CreatedAtRoute<CustomerResponse>> (
            CustomerRequest request,
            CustomerService service,
            CancellationToken cancellationToken) =>
        {
            var customer = await service.CreateAsync(request, cancellationToken);
            return TypedResults.CreatedAtRoute(customer, "GetCustomerById", new { id = customer.Id });
        }).AddEndpointFilter<ValidationFilter<CustomerRequest>>();

        group.MapPut("/{id:guid}", async Task<Results<Ok<CustomerResponse>, NotFound>> (
            Guid id,
            CustomerRequest request,
            CustomerService service,
            CancellationToken cancellationToken) =>
        {
            var customer = await service.UpdateAsync(id, request, cancellationToken);
            return customer is not null
                ? TypedResults.Ok(customer)
                : TypedResults.NotFound();
        }).AddEndpointFilter<ValidationFilter<CustomerRequest>>();

        group.MapPatch("/{id:guid}/archive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            CustomerService service,
            CancellationToken cancellationToken) =>
        {
            return await service.ArchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });

        group.MapPatch("/{id:guid}/unarchive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            CustomerService service,
            CancellationToken cancellationToken) =>
        {
            return await service.UnarchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
