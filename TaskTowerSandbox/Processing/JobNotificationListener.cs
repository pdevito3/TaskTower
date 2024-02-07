namespace TaskTowerSandbox.Processing;

using System.Threading;
using System.Threading.Tasks;
using Configurations;
using Dapper;
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
        
        var pollingInterval = options.JobCheckInterval;
        Log.ForContext("Polling Interval", pollingInterval)
            .Information("Polling for scheduled jobs every {PollingInterval}", pollingInterval);
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
        var scheduledJobs = await conn.QueryAsync<TaskTowerJob>(
            $@"
    SELECT id, payload 
    FROM jobs 
    WHERE status not in ('{JobStatus.Completed().Value}') 
      AND run_after <= @Now
    ORDER BY run_after 
    FOR UPDATE SKIP LOCKED 
    LIMIT 100",
            new { Now = now },
            transaction: tx
        );
        
        // announce the jobs to the job_available channel
        foreach (var job in scheduledJobs)
        {
            await conn.ExecuteAsync($"SELECT pg_notify('job_available', '{job.Id}')");
            Log.Information($"Announced job {job.Id} for processing");
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

