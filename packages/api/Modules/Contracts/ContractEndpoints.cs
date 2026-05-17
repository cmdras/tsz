using Api.Common;
using Api.Common.Extensions;
using Api.Common.Filters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Modules.Contracts;

public static class ContractEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("contracts");

        group.MapGet("/", async (
            string? search,
            ContractSort? sort,
            SortDirection? sortDirection,
            int? page,
            int? pageSize,
            bool? archived,
            ContractService service,
            CancellationToken cancellationToken) =>
        {
            var pageNumber = page is > 0 ? page.Value : 1;
            var size = pageSize is > 0 and <= 100 ? pageSize.Value : 25;
            return TypedResults.Ok(await service.GetAllAsync(
                search,
                sort ?? ContractSort.Number,
                sortDirection ?? SortDirection.Asc,
                pageNumber,
                size,
                archived ?? false,
                cancellationToken));
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<Contract>, NotFound>> (
            Guid id,
            ContractService service,
            CancellationToken cancellationToken) =>
        {
            var contract = await service.GetByIdAsync(id, cancellationToken);
            return contract is not null
                ? TypedResults.Ok(contract)
                : TypedResults.NotFound();
        }).WithName("GetContractById");

        group.MapPost("/", async Task<Results<CreatedAtRoute<Contract>, ProblemHttpResult>> (
            ContractRequest request,
            ContractService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var contract = await service.CreateAsync(request, cancellationToken);
                return TypedResults.CreatedAtRoute(contract, "GetContractById", new { id = contract.Id });
            }
            catch (InvalidContractRequestException exception)
            {
                return TypedResults.Problem(exception.Message, statusCode: 422);
            }
        })
            .AddEndpointFilter<ValidationFilter<ContractRequest>>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", async Task<Results<Ok<Contract>, NotFound, ProblemHttpResult>> (
            Guid id,
            ContractRequest request,
            ContractService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var contract = await service.UpdateAsync(id, request, cancellationToken);
                return contract is not null
                    ? TypedResults.Ok(contract)
                    : TypedResults.NotFound();
            }
            catch (InvalidContractRequestException exception)
            {
                return TypedResults.Problem(exception.Message, statusCode: 422);
            }
        })
            .AddEndpointFilter<ValidationFilter<ContractRequest>>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/archive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            ContractService service,
            CancellationToken cancellationToken) =>
        {
            return await service.ArchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });

        group.MapPatch("/{id:guid}/unarchive", async Task<Results<NoContent, NotFound>> (
            Guid id,
            ContractService service,
            CancellationToken cancellationToken) =>
        {
            return await service.UnarchiveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
