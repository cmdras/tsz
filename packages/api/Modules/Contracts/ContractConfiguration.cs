using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.Contracts;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(contract => contract.Id);

        builder.Property(contract => contract.Number)
            .IsRequired();

        builder.HasIndex(contract => contract.Number)
            .IsUnique();

        builder.Property(contract => contract.CustomerId)
            .IsRequired();

        builder.Property(contract => contract.ConsultantId)
            .IsRequired();

        builder.Property(contract => contract.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(contract => contract.StartDate)
            .IsRequired();

        builder.Property(contract => contract.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(contract => contract.Customer)
            .WithMany()
            .HasForeignKey(contract => contract.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(contract => contract.Consultant)
            .WithMany()
            .HasForeignKey(contract => contract.ConsultantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(contract => contract.Tasks)
            .WithOne(task => task.Contract)
            .HasForeignKey(task => task.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
