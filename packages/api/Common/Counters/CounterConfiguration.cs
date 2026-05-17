using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Common.Counters;

public class CounterConfiguration : IEntityTypeConfiguration<Counter>
{
    public void Configure(EntityTypeBuilder<Counter> builder)
    {
        builder.ToTable("Counters");

        builder.HasKey(counter => counter.Key);

        builder.Property(counter => counter.Key)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(counter => counter.Value)
            .IsRequired();
    }
}
