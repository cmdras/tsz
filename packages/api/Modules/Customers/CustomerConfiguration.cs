using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Number)
            .IsRequired();

        builder.HasIndex(customer => customer.Number)
            .IsUnique();

        builder.Property(customer => customer.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(customer => customer.Street)
            .HasMaxLength(200);

        builder.Property(customer => customer.Zip)
            .HasMaxLength(20);

        builder.Property(customer => customer.City)
            .HasMaxLength(100);

        builder.Property(customer => customer.Country)
            .HasMaxLength(100);

        builder.Property(customer => customer.ContactName)
            .HasMaxLength(200);

        builder.Property(customer => customer.ContactEmail)
            .HasMaxLength(254);

        builder.Property(customer => customer.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
