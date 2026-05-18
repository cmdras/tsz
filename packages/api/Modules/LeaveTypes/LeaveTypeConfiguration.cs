using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.LeaveTypes;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");

        builder.HasKey(leaveType => leaveType.Id);

        builder.Property(leaveType => leaveType.Name)
            .IsRequired()
            .HasMaxLength(100)
            .UseCollation("NOCASE");

        builder.HasIndex(leaveType => leaveType.Name)
            .IsUnique();

        builder.Property(leaveType => leaveType.DefaultDays)
            .IsRequired()
            .HasColumnType("decimal(5,1)");

        builder.Property(leaveType => leaveType.DefaultMode)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(leaveType => leaveType.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
