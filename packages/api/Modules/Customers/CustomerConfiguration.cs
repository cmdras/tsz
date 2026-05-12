using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Number)
            .IsRequired();

        builder.HasIndex(c => c.Number)
            .IsUnique();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Street)
            .HasMaxLength(200);

        builder.Property(c => c.Zip)
            .HasMaxLength(20);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .HasMaxLength(100);

        builder.Property(c => c.ContactName)
            .HasMaxLength(200);

        builder.Property(c => c.ContactEmail)
            .HasMaxLength(254);

        builder.Property(c => c.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
