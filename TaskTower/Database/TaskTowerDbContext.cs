namespace TaskTower.Database;

using Domain.EnqueuedJobs;
using Domain.RunHistories;
using Domain.TaskTowerJob;
using Domain.TaskTowerTags;
using EntityConfigurations;
using Microsoft.EntityFrameworkCore;

public class TaskTowerDbContext(DbContextOptions<TaskTowerDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskTowerJob> Jobs { get; set; }
    public DbSet<EnqueuedJob> EnqueuedJobs { get; set; }
    public DbSet<RunHistory> RunHistories { get; set; }
    public DbSet<TaskTowerTag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new JobConfiguration());
        modelBuilder.ApplyConfiguration(new EnqueuedJobConfiguration());
        modelBuilder.ApplyConfiguration(new RunHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new TaskTowerTagConfiguration());
    }
}