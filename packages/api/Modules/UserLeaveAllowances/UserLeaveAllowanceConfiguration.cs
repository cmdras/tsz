using Api.Modules.LeaveTypes;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.UserLeaveAllowances;

public class UserLeaveAllowanceConfiguration : IEntityTypeConfiguration<UserLeaveAllowance>
{
    public void Configure(EntityTypeBuilder<UserLeaveAllowance> builder)
    {
        builder.ToTable("UserLeaveAllowances");

        builder.HasKey(allowance => allowance.Id);

        builder.Property(allowance => allowance.UserId)
            .IsRequired();

        builder.Property(allowance => allowance.LeaveTypeId)
            .IsRequired();

        builder.Property(allowance => allowance.Year)
            .IsRequired();

        builder.Property(allowance => allowance.Mode)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(allowance => allowance.TotalDays)
            .IsRequired()
            .HasColumnType("decimal(5,1)");

        builder.HasIndex(allowance => new { allowance.UserId, allowance.LeaveTypeId, allowance.Year })
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(allowance => allowance.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LeaveType>()
            .WithMany()
            .HasForeignKey(allowance => allowance.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
