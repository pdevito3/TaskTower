namespace TaskTower.Database.EntityConfigurations;

using Domain.JobStatuses;
using Domain.RunHistories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class RunHistoryConfiguration : IEntityTypeConfiguration<RunHistory>
{
    public void Configure(EntityTypeBuilder<RunHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion(x => x.Value, x => new JobStatus(x))
            .IsRequired();
        builder.Property(x => x.Comment).IsRequired(false);
        builder.Property(x => x.Details).IsRequired(false);
        builder.Property(x => x.OccurredAt).IsRequired();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.JobId);

        builder.HasOne(x => x.Job)
            .WithMany(x => x.RunHistory)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}