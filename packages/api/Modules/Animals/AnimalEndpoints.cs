using Api.Common.Extensions;
using Api.Common.Filters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Modules.Animals;

public static class AnimalEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapApiGroup("animals");

        group.MapGet("/", async (AnimalService service, CancellationToken cancellationToken) =>
            TypedResults.Ok(await service.GetAllAsync(cancellationToken)));

        group.MapGet("/{id:int}", async Task<Results<Ok<Animal>, NotFound>> (int id, AnimalService service, CancellationToken cancellationToken) =>
        {
            var animal = await service.GetByIdAsync(id, cancellationToken);
            return animal is not null
                ? TypedResults.Ok(animal)
                : TypedResults.NotFound();
        });

        group.MapPost("/", async Task<Created<Animal>> (CreateAnimalRequest request, AnimalService service, CancellationToken cancellationToken) =>
        {
            var animal = await service.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/animals/{animal.Id}", animal);
        }).AddEndpointFilter<ValidationFilter<CreateAnimalRequest>>();

        group.MapPut("/{id:int}", async Task<Results<Ok<Animal>, NotFound>> (int id, UpdateAnimalRequest request, AnimalService service, CancellationToken cancellationToken) =>
        {
            var animal = await service.UpdateAsync(id, request, cancellationToken);
            return animal is not null
                ? TypedResults.Ok(animal)
                : TypedResults.NotFound();
        }).AddEndpointFilter<ValidationFilter<UpdateAnimalRequest>>();

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, AnimalService service, CancellationToken cancellationToken) =>
        {
            return await service.DeleteAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
