namespace TaskTowerSandbox.Processing;

using System.Threading;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Domain.EnqueuedJobs;
using Domain.RunHistories;
using Domain.RunHistories.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using TaskTowerSandbox.Domain.JobStatuses;
using TaskTowerSandbox.Domain.TaskTowerJob;
using Utils;

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
        Log.Debug("Task Tower worker is starting");
        using var scope = _serviceScopeFactory.CreateScope();
        
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        Log.Debug("Subscribing to job_available channel");
        await using (var cmd = new NpgsqlCommand("LISTEN job_available", conn))
        {
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        Log.Debug("Subscribed to job_available channel");
        
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
                    Log.Debug("Notification received for queue {Queue} with Job ID {Id}", queuePart, idPart);
                    
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
        Log.Debug("Polling for scheduled jobs every {PollingInterval}", pollingInterval);
        await using var timer = new Timer(async _ =>
            {
                await _semaphore.WaitAsync(stoppingToken);
                try
                {
                    await EnqueueScheduledJobs(stoppingToken);
                }
                finally
                {
                    _semaphore.Release();
                }
            },
            null, TimeSpan.Zero, pollingInterval);
        
        // poll for enqueued jobs -- this doesn't work great
        var enqueuedJobsInterval = TimeSpan.FromSeconds(1);
        Log.Debug("Polling for enqueued jobs every {EnqueuedJobsInterval}", enqueuedJobsInterval);
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

        if (_options.IdleTransactionTimeout > 0)
        {
            await using var cmd = new NpgsqlCommand($"SET idle_in_transaction_session_timeout TO {_options.IdleTransactionTimeout};", conn);
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        var enqueuedJobs = await conn.QueryAsync<EnqueuedJob>(
            $@"
    SELECT job_id as JobId, queue as Queue
    FROM enqueued_jobs
    FOR UPDATE SKIP LOCKED 
    LIMIT 8000",
            transaction: tx
        );
        
        foreach (var enqueuedJob in enqueuedJobs)
        {
            var notifyPayload = $"Queue: {enqueuedJob.Queue}, ID: {enqueuedJob.JobId}";
            await conn.ExecuteAsync("SELECT pg_notify('job_available', @Payload)",
                new { Payload = notifyPayload },
                transaction: tx
            );
            Log.Debug("Announced job {JobId} to job_available channel from the queue", enqueuedJob.JobId);
        }
        
        await tx.CommitAsync(stoppingToken);
    }
    
    private async Task EnqueueScheduledJobs(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        if (_options.IdleTransactionTimeout > 0)
        {
            await using var cmd = new NpgsqlCommand($"SET idle_in_transaction_session_timeout TO {_options.IdleTransactionTimeout};", conn);
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);

        var queuePrioritization = _options.QueuePrioritization;
        var scheduledJobs = await queuePrioritization.GetJobsToEnqueue(conn, tx, 
            _options.QueuePriorities);
        
        foreach (var job in scheduledJobs)
        {
            var insertResult = await conn.ExecuteAsync(
                "INSERT INTO enqueued_jobs(id, job_id, queue) VALUES (gen_random_uuid(), @Id, @Queue)",
                new { job.Id, job.Queue },
                transaction: tx
            );
            
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
        // TODO add connection timeout handling
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        // TODO add transaction timeout handling
        if (_options.IdleTransactionTimeout > 0)
        {
            await using var cmd = new NpgsqlCommand($"SET idle_in_transaction_session_timeout TO {_options.IdleTransactionTimeout};", conn);
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        var queuePrioritization = _options.QueuePrioritization;
        var job = await queuePrioritization.GetJobToRun(conn, tx, _options.QueuePriorities);
        
        if (job != null)
        {
            var now = DateTimeOffset.UtcNow;
            var runHistoryProcessing = RunHistory.Create(new RunHistoryForCreation()
            {
                JobId = job.Id,
                Status = JobStatus.Processing()
            });
            // TODO add enqueue history on trigger? and on method
            
            await AddRunHistory(conn, runHistoryProcessing, tx);
            
            Log.Debug("Processing job {JobId} from queue {Queue} with payload {Payload} at {Now}", job.Id, job.Queue, job.Payload, now.ToString("o"));
            
            // var delay = new Random().Next(500, 5000);
            var delay = 0;
            await Task.Delay(delay, stoppingToken); 
            
            var success = new Random().Next(0, 100) < 20;
            if (success)
            {
                var updateResult = await conn.ExecuteAsync(
                    $"UPDATE jobs SET status = @Status, ran_at = @Now WHERE id = @Id",
                    new { job.Id, Status = JobStatus.Completed().Value, now },
                    transaction: tx
                );
                
                var deleteEnqueuedJobResult = await conn.ExecuteAsync(
                    "DELETE FROM enqueued_jobs WHERE job_id = @Id",
                    new { job.Id },
                    transaction: tx
                );
                
                var runHistory = RunHistory.Create(new RunHistoryForCreation()
                {
                    JobId = job.Id,
                    Status = JobStatus.Completed()
                });
                await AddRunHistory(conn, runHistory, tx);
            }
            else
            {
                var nextRunAt = BackoffCalculator.CalculateBackoff(job.Retries);
                var updateResult = await conn.ExecuteAsync(
                    $"UPDATE jobs SET status = @Status, ran_at = @now, run_after = @RunAfter, retries = retries + 1 WHERE id = @Id",
                    new { job.Id, Status = JobStatus.Failed().Value, RunAfter = nextRunAt, now },
                    transaction: tx
                );
                
                var deleteEnqueuedJobResult = await conn.ExecuteAsync(
                    "DELETE FROM enqueued_jobs WHERE job_id = @Id",
                    new { job.Id },
                    transaction: tx
                );
                
                var runHistory = RunHistory.Create(new RunHistoryForCreation()
                {
                    JobId = job.Id,
                    Status = JobStatus.Failed(),
                    Comment = "some tbd error",
                    Details = "some tbd details"
                });
                await AddRunHistory(conn, runHistory, tx);
                
                Log.Debug("Job {JobId} failed", job.Id);
            }
            
            Log.Debug("Processed job {JobId} from queue {Queue} with payload {Payload} in {Delay}ms, finishing at {Time}", job.Id, job.Queue, job.Payload, delay, DateTimeOffset.UtcNow.ToString("o"));
        }
        
        await tx.CommitAsync(stoppingToken);
    }

    private static async Task AddRunHistory(NpgsqlConnection conn, RunHistory runHistory, NpgsqlTransaction tx)
    {
        await conn.ExecuteAsync(
            "INSERT INTO run_histories(id, job_id, status, comment, details, occurred_at) VALUES (@Id, @JobId, @Status, @Comment, @Details, @OccurredAt)",
            new { runHistory.Id, runHistory.JobId, Status = runHistory.Status.Value, runHistory.Comment, runHistory.Details, runHistory.OccurredAt },
            transaction: tx
        );
    }

    public override void Dispose()
    {
        Log.Debug("Task Tower worker is shutting down");
        _semaphore.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}

