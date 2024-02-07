namespace TaskTowerSandbox.Processing;

using System.Threading;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using TaskTowerSandbox.Domain.JobStatuses;
using TaskTowerSandbox.Domain.TaskTowerJob;

public class JobNotificationListener(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Task Tower worker is starting");
        var options = serviceScopeFactory.CreateScope()
            .ServiceProvider.GetRequiredService<IOptions<TaskTowerOptions>>().Value;
        
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        Log.Information("Subscribing to job_available channel");
        await using (var cmd = new NpgsqlCommand("LISTEN job_available", conn))
        {
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        Log.Information("Subscribed to job_available channel");

        conn.Notification += async (_, e) =>
        {
            Log.Information("Notification received: Job available with ID {JobId}", e.Payload);
            await ProcessJob(stoppingToken, options);
        };
        
        var pollingInterval = TimeSpan.FromSeconds(2);
        await using var timer = new Timer(async _ =>
            {
                await ProcessScheduledJobs(stoppingToken, options);
            },
            null, TimeSpan.Zero, pollingInterval);

        
        while (!stoppingToken.IsCancellationRequested)
        {
            // This call is blocking until a notification is received
            await conn.WaitAsync(stoppingToken);
        }
    }
    
    private async Task ProcessScheduledJobs(CancellationToken stoppingToken, TaskTowerOptions options)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
    
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
    
        var now = DateTimeOffset.UtcNow;
    
        // Fetch the next scheduled job that is ready to run and not already locked by another process
        var scheduledJob = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
            $@"
            SELECT id, payload 
            FROM jobs 
            WHERE status not in ('{JobStatus.Completed().Value}') 
              AND run_after <= @Now
            ORDER BY run_after 
            FOR UPDATE SKIP LOCKED 
            LIMIT 1",
            new { Now = now },
            transaction: tx
        );
        
        Log.Information("scheduledJob: {@ScheduledJob}", scheduledJob);
        
        if (scheduledJob != null)
        {
            Log.Information($"Processing scheduled job {scheduledJob.Id} with payload {scheduledJob.Payload}");
            // Process the job here
            // Depending on your logic, this might involve executing the job's payload or updating its status in the database
        
            // TODO leverage domain for logic
            var updateResult = await conn.ExecuteAsync(
                $"UPDATE jobs SET status = '{JobStatus.Completed().Value}', ran_at = @Now WHERE id = @Id",
                new { Id = scheduledJob.Id, Now = now },
                transaction: tx
            );
        }
    
        await tx.CommitAsync(stoppingToken);
    }

    private async Task ProcessJob(CancellationToken stoppingToken, TaskTowerOptions options)
    {
        await using var conn = new NpgsqlConnection(options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        // Fetch the next available job that is not already locked by another process
        var job = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
            $@"
                SELECT id, payload
                FROM jobs 
                WHERE status not in ('{JobStatus.Completed().Value}') 
                ORDER BY created_at 
                FOR UPDATE SKIP LOCKED 
                LIMIT 1",
            transaction: tx
        );
        
        if (job != null)
        {
            Log.Information($"Processing job {job.Id} with payload {job.Payload}");
            
            // TODO leverage domain for logic
            var now = DateTimeOffset.UtcNow;
            var updateResult = await conn.ExecuteAsync(
                $"UPDATE jobs SET status = '{JobStatus.Completed().Value}', ran_at = @Now WHERE id = @Id",
                new { job.Id, now },
                transaction: tx
            );
        }
        
        await tx.CommitAsync(stoppingToken);
    }
    
    public override void Dispose()
    {
        Log.Information("Task Tower worker is shutting down");
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}

