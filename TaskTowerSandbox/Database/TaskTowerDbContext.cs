namespace TaskTowerSandbox.Database;

using System.Data.Common;
using Domain.JobStatuses;
using Domain.TaskTowerJob;
using EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TaskTowerDbContext(DbContextOptions<TaskTowerDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskTowerJob> Jobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new JobConfiguration());
    }
}

public class ForUpdateSkipLockedInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        if (command.CommandText.Contains(Consts.RowLockTag))
        {
            command.CommandText += " FOR UPDATE SKIP LOCKED";
        }

        return base.ReaderExecuting(command, eventData, result);
    }
}