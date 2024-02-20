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

        Log.Debug($"Subscribing to {Consts.Notifications.JobAvailable} channel");
        await using (var cmd = new NpgsqlCommand($"LISTEN {Consts.Notifications.JobAvailable}", conn))
        {
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        Log.Debug($"Subscribed to {Consts.Notifications.JobAvailable} channel");
        
        // Define the action to take when a notification is received
        conn.Notification += async (_, e) =>
        {
            // var channel = e.Channel;
            
            var parsedPayload = NotificationHelper.ParsePayload(e.Payload);

            if (!string.IsNullOrEmpty(parsedPayload.Queue) && parsedPayload.JobId != Guid.Empty)
            {
                Log.Debug("Notification received for queue {Queue} with Job ID {Id}", parsedPayload.Queue, parsedPayload.JobId);
                    
                // Wait to enter the semaphore before processing a job
                await _semaphore.WaitAsync(stoppingToken);
                try
                {
                    await using var notificationScope = _serviceScopeFactory.CreateAsyncScope();
                    await ProcessAvailableJob(notificationScope.ServiceProvider, stoppingToken);
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
        
        var enqueuedJobsInterval = TimeSpan.FromSeconds(1);
        Log.Debug("Polling for enqueued jobs every {EnqueuedJobsInterval}", enqueuedJobsInterval);
        await using var enqueuedJobsTimer = new Timer(async _ =>
            {
                await _semaphore.WaitAsync(stoppingToken);
                try
                {
                    await AnnounceEnqueuedJobs(_options.QueuePriorities,
                        stoppingToken);
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
    
    private async Task AnnounceEnqueuedJobs(Dictionary<string, int> queuePriorities, CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        if (_options.IdleTransactionTimeout > 0)
        {
            await using var cmd = new NpgsqlCommand($"SET idle_in_transaction_session_timeout TO {_options.IdleTransactionTimeout};", conn);
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        // Construct the CASE statement for queue priorities
        var priorityCaseSql = string.Join(" ",
            queuePriorities.Select((kvp, index) => $"WHEN '{kvp.Key}' THEN {kvp.Value}"));

        // Append the CASE statement to ORDER BY if it's not empty; otherwise, default to ordering by some other column
        if (!string.IsNullOrEmpty(priorityCaseSql))
        {
            priorityCaseSql = $"CASE queue {priorityCaseSql} ELSE 0 END";
        }
        else
        {
            // Fallback ordering, e.g., by id or run_after, if no priority case statement is constructed
            priorityCaseSql =
                "id"; // Adjust this line according to your table's structure and desired default ordering
        }
        
        var enqueuedJobs = await _options.QueuePrioritization.GetEnqueuedJobs(conn, tx, queuePriorities, 8000);

        foreach (var enqueuedJob in enqueuedJobs)
        {
            var notifyPayload = NotificationHelper.CreatePayload(enqueuedJob.Queue, enqueuedJob.JobId);
            await conn.ExecuteAsync($"SELECT pg_notify(@Notification, @Payload)",
                new { Notification = Consts.Notifications.JobAvailable, Payload = notifyPayload },
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
                new { job.Id, Status = JobStatus.Enqueued().Value },
                transaction: tx
            );
            
            var runHistory = RunHistory.Create(new RunHistoryForCreation()
            {
                JobId = job.Id,
                Status = JobStatus.Enqueued()
            });
            await AddRunHistory(conn, runHistory, tx);
        }
    
        await tx.CommitAsync(stoppingToken);
    }

    private async Task ProcessAvailableJob(IServiceProvider serviceProvider, CancellationToken stoppingToken)
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
            var nowProcessing = DateTimeOffset.UtcNow;

            Log.Debug("Processing job {JobId} from queue {Queue} with payload {Payload} at {Now}", job.Id, job.Queue, job.Payload, nowProcessing.ToString("o"));

            try
            {
                await job.Invoke(serviceProvider);
                var runHistoryProcessing = RunHistory.Create(new RunHistoryForCreation()
                {
                    JobId = job.Id,
                    Status = JobStatus.Processing(),
                    OccurredAt = nowProcessing
                });
                await AddRunHistory(conn, runHistoryProcessing, tx);
                
                var nowDone = DateTimeOffset.UtcNow;
                var updateResult = await conn.ExecuteAsync(
                    $"UPDATE jobs SET status = @Status, ran_at = @Now WHERE id = @Id",
                    new { job.Id, Status = JobStatus.Completed().Value, Now = nowDone },
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
                    Status = JobStatus.Completed(),
                    OccurredAt = nowDone
                });
                await AddRunHistory(conn, runHistory, tx);
            }
            catch (Exception ex)
            {
                job.MarkAsFailed();
                var updateResult = await conn.ExecuteAsync(
                    @$"UPDATE jobs 
    SET 
        status = @Status, 
        type = @Type, 
        method = @Method, 
        parameter_types = @ParameterTypes, 
        payload = @Payload::jsonb, 
        retries = @Retries, 
        max_retries = @MaxRetries, 
        run_after = @RunAfter, 
        ran_at = @RanAt, 
        created_at = @CreatedAt, 
        deadline = @Deadline
    WHERE id = @Id",
                    new { 
                        Id = job.Id, 
                        Status = job.Status.Value, 
                        Type = job.Type,
                        Method = job.Method,
                        ParameterTypes = job.ParameterTypes,
                        Payload = job.Payload,
                        Retries = job.Retries,
                        MaxRetries = job.MaxRetries,
                        RunAfter = job.RunAfter,
                        RanAt = job.RanAt,
                        CreatedAt = job.CreatedAt,
                        Deadline = job.Deadline
                    },
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
                    Comment = ex.Message,
                    Details = ex.StackTrace,
                    OccurredAt = job.RanAt ?? DateTimeOffset.UtcNow
                });
                await AddRunHistory(conn, runHistory, tx);
                
                Log.Error("Job {JobId} failed because of {Reasons}", job.Id, ex.Message);
                if (job.Status.IsDead())
                {
                    Log.Error("Job {JobId} is dead", job.Id);
                }
            }
            await tx.CommitAsync(stoppingToken);
            
            Log.Debug("Processed job {JobId} from queue {Queue} with payload {Payload}, finishing at {Time}", job.Id, job.Queue, job.Payload, DateTimeOffset.UtcNow.ToString("o"));
        }
        
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

