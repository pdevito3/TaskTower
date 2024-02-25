namespace TaskTower.Database.EntityConfigurations;

using System.Text.Json;
using Domain.JobStatuses;
using Domain.TaskTowerJob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class JobConfiguration : IEntityTypeConfiguration<TaskTowerJob>
{
    public void Configure(EntityTypeBuilder<TaskTowerJob> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Queue).IsRequired(false);
        builder.Property(x => x.Status)
            .HasConversion(x => x.Value, x => new JobStatus(x))
            .IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Method).IsRequired();
        builder.Property(x => x.ParameterTypes)
            .HasColumnType("text[]");
        builder.Property(x => x.Payload)
            .HasColumnType("jsonb");
        builder.Property(x => x.Retries).IsRequired();

        // TODO see how ef 8 would handle a json col like this? guessing it's still just jsonb with what amounts to a conversion? and dapper needs to like it anyway
        builder.Ignore(x => x.ContextParameters);
        builder.Property(x => x.RawContextParameters)
            .HasColumnName("context_parameters")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.MaxRetries).IsRequired(false);
        builder.Property(x => x.RunAfter).IsRequired();
        builder.Property(x => x.RanAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Deadline).IsRequired(false);
        
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.RunAfter);
    }
}