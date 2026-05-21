using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Modules.TimeEntries;

public class WeekSubmissionConfiguration : IEntityTypeConfiguration<WeekSubmission>
{
    public void Configure(EntityTypeBuilder<WeekSubmission> builder)
    {
        builder.ToTable("WeekSubmissions");

        builder.HasKey(weekSubmission => weekSubmission.Id);

        builder.Property(weekSubmission => weekSubmission.UserId).IsRequired();

        builder.Property(weekSubmission => weekSubmission.WeekStart).IsRequired();

        builder.Property(weekSubmission => weekSubmission.SubmittedAt).IsRequired();

        builder.HasIndex(weekSubmission => new { weekSubmission.UserId, weekSubmission.WeekStart })
            .IsUnique();

        builder.HasOne(weekSubmission => weekSubmission.User)
            .WithMany()
            .HasForeignKey(weekSubmission => weekSubmission.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
