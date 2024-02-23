namespace TaskTowerSandbox.Database.EntityConfigurations;

using Domain.EnqueuedJobs;
using Domain.JobStatuses;
using Domain.TaskTowerJob;
using Domain.TaskTowerTags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class TaskTowerTagConfiguration : IEntityTypeConfiguration<TaskTowerTag>
{
    public void Configure(EntityTypeBuilder<TaskTowerTag> builder)
    {
        builder.HasKey(t => new { t.JobId, t.Name });
        builder.Property(x => x.Name).IsRequired();
        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.Job)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}