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

public class JobNotificationListener : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SemaphoreSlim _semaphore;
    private readonly TaskTowerOptions _options;

    public JobNotificationListener(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        
        _options = _serviceScopeFactory.CreateScope()
            .ServiceProvider.GetRequiredService<IOptions<TaskTowerOptions>>().Value;
        
        if(_options == null)
            throw new ArgumentNullException("No TaskTowerOptions were found in the service provider");
        
        _semaphore = new SemaphoreSlim(_options.BackendConcurrency);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Task Tower worker is starting");
        using var scope = _serviceScopeFactory.CreateScope();
        
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        Log.Information("Subscribing to job_available channel");
        await using (var cmd = new NpgsqlCommand("LISTEN job_available", conn))
        {
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        Log.Information("Subscribed to job_available channel");
        
        // Define the action to take when a notification is received
        conn.Notification += async (_, e) =>
        {
            var payload = e.Payload;
            var parts = payload.Split(new[] { ", ID: " }, StringSplitOptions.None);
            var queuePart = parts[0].Substring("Queue: ".Length);
            var idPart = parts.Length > 1 ? parts[1] : string.Empty;

            if (!string.IsNullOrEmpty(queuePart) && !string.IsNullOrEmpty(idPart))
            {
                // Log.Information("Notification received: Job available with ID {JobId}", e.Payload);
                Log.Information("Notification received for queue {Queue} with Job ID {Id}", queuePart, idPart);
                
                // Wait to enter the semaphore before processing a job
                await _semaphore.WaitAsync(stoppingToken);
                try
                {
                    await ProcessAvailableJob(stoppingToken);
                }
                finally
                {
                    // Ensure the semaphore is always released
                    _semaphore.Release();
                }
            }
        };
        
        // Configure a timer to poll for scheduled jobs at the specified interval
        var pollingInterval = _options.JobCheckInterval;
        Log.Information("Polling for scheduled jobs every {PollingInterval}", pollingInterval);
        await using var timer = new Timer(async _ =>
            {
                await _semaphore.WaitAsync(stoppingToken);
                try
                {
                    await ProcessScheduledJobs(stoppingToken);
                }
                finally
                {
                    _semaphore.Release();
                }
            },
            null, TimeSpan.Zero, pollingInterval);
        
        // Keep the service running until a cancellation request is received
        while (!stoppingToken.IsCancellationRequested)
        {
            // This call is blocking until a notification is received
            await conn.WaitAsync(stoppingToken);
        }
    }
    
    private async Task ProcessScheduledJobs(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
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
            await conn.ExecuteAsync($"SELECT pg_notify('job_available', 'Queue: @Queue, ID: @Id')",
                new { job.Id, job.Queue },
                transaction: tx
            );
            Log.Information($"Announced job {job.Id} for processing");
        }
    
        await tx.CommitAsync(stoppingToken);
    }

    private async Task ProcessAvailableJob(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
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
        _semaphore.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}

