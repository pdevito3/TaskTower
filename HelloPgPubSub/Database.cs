using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelloPgPubSub;

using Microsoft.EntityFrameworkCore;

public record JobData(string Payload);

public sealed record Job
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public string Payload { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Job> Jobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new JobConfiguration());
    }
}

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.Property(x => x.Payload)
            .HasColumnType("jsonb");
    }
}