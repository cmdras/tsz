using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(user => user.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
