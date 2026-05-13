
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Animals;

public class AnimalService
{
    private readonly AnimalDbContext _dbContext;

    public AnimalService(AnimalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Animal>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Animals.ToListAsync(cancellationToken);

    public Task<Animal?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Animals.FindAsync([id], cancellationToken).AsTask();

    public async Task<Animal> CreateAsync(CreateAnimalRequest request, CancellationToken cancellationToken = default)
    {
        var animal = new Animal
        {
            Name = request.Name,
            Species = request.Species,
            Age = request.Age
        };
        await _dbContext.Animals.AddAsync(animal, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return animal;
    }

    public async Task<Animal?> UpdateAsync(int id, UpdateAnimalRequest request, CancellationToken cancellationToken = default)
    {
        var animal = await _dbContext.Animals.FindAsync([id], cancellationToken);
        if (animal is null) return null;

        animal.Name = request.Name;
        animal.Species = request.Species;
        animal.Age = request.Age;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return animal;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var animal = await _dbContext.Animals.FindAsync([id], cancellationToken);
        if (animal is null) return false;

        _dbContext.Animals.Remove(animal);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
