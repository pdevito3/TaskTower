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
            var channel = e.Channel;
            if (channel == "job_available")
            {
                var payload = e.Payload;
                var parts = payload.Split(new[] { ", ID: " }, StringSplitOptions.None);
                var queuePart = parts[0].Substring("Queue: ".Length);
                var idPart = parts.Length > 1 ? parts[1] : string.Empty;

                if (!string.IsNullOrEmpty(queuePart) && !string.IsNullOrEmpty(idPart))
                {
                    // Log.Information("Notification received for queue {Queue} with Job ID {Id}", queuePart, idPart);
                    
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
        
        // poll for enqueued jobs -- this doesn't work great
        var enqueuedJobsInterval = TimeSpan.FromSeconds(1);
        Log.Information("Polling for enqueued jobs every {EnqueuedJobsInterval}", enqueuedJobsInterval);
        await using var enqueuedJobsTimer = new Timer(async _ =>
            {
                await _semaphore.WaitAsync(stoppingToken);
                try
                {
                    await AnnounceEnqueuedJobs(stoppingToken);
                }
                finally
                {
                    _semaphore.Release();
                }
            },
            null, TimeSpan.Zero, enqueuedJobsInterval);
        
        // Keep the service running until a cancellation request is received
        while (!stoppingToken.IsCancellationRequested)
        {
            // This call is blocking until a notification is received
            await conn.WaitAsync(stoppingToken);
        }
    }
    
    private async Task AnnounceEnqueuedJobs(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        var enqueuedJobs = await conn.QueryAsync<EnqueuedJob>(
            $@"
    SELECT job_id as JobId, queue as Queue
    FROM enqueued_jobs
    FOR UPDATE SKIP LOCKED 
    LIMIT 5000",
            transaction: tx
        );
        
        foreach (var enqueuedJob in enqueuedJobs)
        {
            var notifyPayload = $"Queue: {enqueuedJob.Queue}, ID: {enqueuedJob.Id}";
            await conn.ExecuteAsync("SELECT pg_notify('job_available', @Payload)",
                new { Payload = notifyPayload },
                transaction: tx
            );
            // Log.Information("Announced job {JobId} to job_available channel from the queue", enqueuedJob.JobId);
        }
        
        await tx.CommitAsync(stoppingToken);
    }
    
    private async Task ProcessScheduledJobs(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
    
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
    
        var now = DateTimeOffset.UtcNow;
        var scheduledJobs = await conn.QueryAsync<TaskTowerJob>(
            $@"
    SELECT id, queue
    FROM jobs 
    WHERE status in (@Status) 
      AND run_after <= @Now
    ORDER BY run_after 
    FOR UPDATE SKIP LOCKED 
    LIMIT 8000",
            // TODO add failed
            new { Now = now, Status = JobStatus.Pending().Value },
            transaction: tx
        );
        
        foreach (var job in scheduledJobs)
        {
            var insertResult = await conn.ExecuteAsync(
                "INSERT INTO enqueued_jobs(id, job_id, queue) VALUES (gen_random_uuid(), @Id, @Queue)",
                new { job.Id, job.Queue },
                transaction: tx
            );
            // also update job status to processing
            var updateResult = await conn.ExecuteAsync(
                $"UPDATE jobs SET status = @Status WHERE id = @Id",
                new { job.Id, Status = JobStatus.Processing().Value },
                transaction: tx
            );
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
                SELECT id, payload, queue
                FROM jobs 
                WHERE status not in (@Status) 
                ORDER BY created_at 
                FOR UPDATE SKIP LOCKED 
                LIMIT 1",
            new { Status = JobStatus.Completed().Value },
            transaction: tx
        );
        
        if (job != null)
        {
            Log.Information("Processing job {JobId} from queue {Queue} with payload {Payload}", job.Id, job.Queue, job.Payload); 
            
            // TODO leverage domain for logic
            var now = DateTimeOffset.UtcNow;
            var updateResult = await conn.ExecuteAsync(
                $"UPDATE jobs SET status = '{JobStatus.Completed().Value}', ran_at = @Now WHERE id = @Id",
                new { job.Id, now },
                transaction: tx
            );
            
            var deleteEnqueuedJobResult = await conn.ExecuteAsync(
                "DELETE FROM enqueued_jobs WHERE job_id = @Id",
                new { job.Id },
                transaction: tx
            );
            
            // Log.Information("Processed job {JobId} from queue {Queue} with payload {Payload}", job.Id, job.Queue, job.Payload);
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

