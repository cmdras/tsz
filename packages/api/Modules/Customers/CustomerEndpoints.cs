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
            CustomerService service,
            CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(search, ct)));

        group.MapGet("/{id:guid}", async Task<Results<Ok<Customer>, NotFound>> (
            Guid id,
            CustomerService service,
            CancellationToken ct) =>
        {
            var customer = await service.GetByIdAsync(id, ct);
            return customer is not null
                ? TypedResults.Ok(customer)
                : TypedResults.NotFound();
        }).WithName("GetCustomerById");

        group.MapPost("/", async Task<CreatedAtRoute<Customer>> (
            CustomerRequest request,
            CustomerService service,
            CancellationToken ct) =>
        {
            var customer = await service.CreateAsync(request, ct);
            return TypedResults.CreatedAtRoute(customer, "GetCustomerById", new { id = customer.Id });
        }).AddEndpointFilter<ValidationFilter<CustomerRequest>>();

        group.MapPut("/{id:guid}", async Task<Results<Ok<Customer>, NotFound>> (
            Guid id,
            CustomerRequest request,
            CustomerService service,
            CancellationToken ct) =>
        {
            var customer = await service.UpdateAsync(id, request, ct);
            return customer is not null
                ? TypedResults.Ok(customer)
                : TypedResults.NotFound();
        }).AddEndpointFilter<ValidationFilter<CustomerRequest>>();

        group.MapPatch("/{id:guid}/archive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            CustomerService service,
            CancellationToken ct) =>
        {
            return await service.ArchiveAsync(id, ct)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });

        group.MapPatch("/{id:guid}/unarchive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            CustomerService service,
            CancellationToken ct) =>
        {
            return await service.UnarchiveAsync(id, ct)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
