using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.Contracts;

public class ContractTaskConfiguration : IEntityTypeConfiguration<ContractTask>
{
    public void Configure(EntityTypeBuilder<ContractTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.ContractId)
            .IsRequired();

        builder.Property(task => task.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(task => task.DayRate)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(task => task.Order)
            .IsRequired();

        builder.Property(task => task.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
