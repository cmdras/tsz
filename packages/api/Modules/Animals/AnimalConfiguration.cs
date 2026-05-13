using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.Animals;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.ToTable("Animals");

        builder.HasKey(animal => animal.Id);

        builder.Property(animal => animal.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(animal => animal.Species)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(animal => animal.Age)
            .IsRequired();
    }
}
