namespace TaskTowerSandbox.Database.EntityConfigurations;

using Domain.JobStatuses;
using Domain.TaskTowerJob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class JobConfiguration : IEntityTypeConfiguration<TaskTowerJob>
{
    public void Configure(EntityTypeBuilder<TaskTowerJob> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Fingerprint).IsRequired();
        builder.Property(x => x.Queue).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion(x => x.Value, x => new JobStatus(x))
            .IsRequired();
        builder.Property(x => x.Payload)
            .HasColumnType("jsonb");
        builder.Property(x => x.Retries).IsRequired();
        builder.Property(x => x.MaxRetries).IsRequired(false);
        builder.Property(x => x.RunAfter).IsRequired();
        builder.Property(x => x.RanAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Error).IsRequired(false);
        builder.Property(x => x.Deadline).IsRequired(false);
    }
}