namespace TaskTowerSandbox.Database;

using Domain.EnqueuedJobs;
using Domain.TaskTowerJob;
using EntityConfigurations;
using Microsoft.EntityFrameworkCore;

public class TaskTowerDbContext(DbContextOptions<TaskTowerDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskTowerJob> Jobs { get; set; }
    public DbSet<EnqueuedJob> EnqueuedJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new JobConfiguration());
        modelBuilder.ApplyConfiguration(new EnqueuedJobConfiguration());
    }
}