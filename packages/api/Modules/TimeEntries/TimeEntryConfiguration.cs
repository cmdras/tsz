using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.TimeEntries;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("TimeEntries", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_TimeEntries_ExactlyOneFK",
                "(ContractTaskId IS NULL) != (LeaveTypeId IS NULL)");
        });

        builder.HasKey(timeEntry => timeEntry.Id);

        builder.Property(timeEntry => timeEntry.UserId).IsRequired();

        builder.Property(timeEntry => timeEntry.Date).IsRequired();

        builder.Property(timeEntry => timeEntry.Hours)
            .IsRequired()
            .HasColumnType("decimal(4,1)");

        builder.Property(timeEntry => timeEntry.UpdatedAt).IsRequired();

        builder.HasIndex(timeEntry => new { timeEntry.UserId, timeEntry.Date, timeEntry.ContractTaskId })
            .IsUnique()
            .HasFilter("ContractTaskId IS NOT NULL");

        builder.HasIndex(timeEntry => new { timeEntry.UserId, timeEntry.Date, timeEntry.LeaveTypeId })
            .IsUnique()
            .HasFilter("LeaveTypeId IS NOT NULL");

        builder.HasOne(timeEntry => timeEntry.User)
            .WithMany()
            .HasForeignKey(timeEntry => timeEntry.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(timeEntry => timeEntry.ContractTask)
            .WithMany()
            .HasForeignKey(timeEntry => timeEntry.ContractTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(timeEntry => timeEntry.LeaveType)
            .WithMany()
            .HasForeignKey(timeEntry => timeEntry.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
