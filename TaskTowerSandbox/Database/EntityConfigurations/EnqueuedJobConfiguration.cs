namespace TaskTowerSandbox.Database.EntityConfigurations;

using Domain.JobStatuses;
using Domain.TaskTowerJob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class EnqueuedJobConfiguration : IEntityTypeConfiguration<EnqueuedJob>
{
    public void Configure(EntityTypeBuilder<EnqueuedJob> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Queue).IsRequired();
        
        builder.HasOne(x => x.Job)
               .WithOne(x => x.EnqueuedJob)
               .HasForeignKey<EnqueuedJob>(x => x.JobId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}