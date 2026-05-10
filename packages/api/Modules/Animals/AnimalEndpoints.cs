using Api.Common.Extensions;
using Api.Common.Filters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Modules.Animals;

public static class AnimalEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("animals");

        group.MapGet("/", async (AnimalService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:int}", async Task<Results<Ok<Animal>, NotFound>> (int id, AnimalService service, CancellationToken ct) =>
        {
            var animal = await service.GetByIdAsync(id, ct);
            return animal is not null
                ? TypedResults.Ok(animal)
                : TypedResults.NotFound();
        });

        group.MapPost("/", async Task<Created<Animal>> (CreateAnimalRequest request, AnimalService service, CancellationToken ct) =>
        {
            var animal = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/animals/{animal.Id}", animal);
        }).AddEndpointFilter<ValidationFilter<CreateAnimalRequest>>();

        group.MapPut("/{id:int}", async Task<Results<Ok<Animal>, NotFound>> (int id, UpdateAnimalRequest request, AnimalService service, CancellationToken ct) =>
        {
            var animal = await service.UpdateAsync(id, request, ct);
            return animal is not null
                ? TypedResults.Ok(animal)
                : TypedResults.NotFound();
        }).AddEndpointFilter<ValidationFilter<UpdateAnimalRequest>>();

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, AnimalService service, CancellationToken ct) =>
        {
            return await service.DeleteAsync(id, ct)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
