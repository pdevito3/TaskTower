namespace TaskTower.Processing;

using System.Threading;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Database;
using Domain.InterceptionStages;
using Domain.JobStatuses;
using Domain.RunHistories;
using Domain.RunHistories.Models;
using Domain.RunHistories.Services;
using Domain.RunHistoryStatuses;
using Domain.TaskTowerJob;
using Exceptions;
using Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Utils;

public class JobNotificationListener : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SemaphoreSlim _semaphore;
    private readonly TaskTowerOptions _options;
    private readonly ILogger _logger;
    private readonly Guid _workerId = Guid.NewGuid();
    private const string _announcingSqlComment = "This query will commit the transaction for announcing jobs.";
    
    public JobNotificationListener(IServiceScopeFactory serviceScopeFactory, ILogger<JobNotificationListener> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        _options = _serviceScopeFactory.CreateScope()
            .ServiceProvider.GetRequiredService<IOptions<TaskTowerOptions>>().Value;

        if (_options == null)
            throw new MissingTaskTowerOptionsException();
        
        _semaphore = new SemaphoreSlim(_options.BackendConcurrency);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // try
        // {
        _logger.LogInformation("Task Tower worker is starting");
        _logger.LogDebug("Task Tower worker is using a concurrency level of {Concurrency}", _options.BackendConcurrency);
        using var scope = _serviceScopeFactory.CreateScope();

        await ResetMidProcessingJobs(stoppingToken);
        
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        _logger.LogInformation("Subscribing to {Channel} channel", TaskTowerConstants.Notifications.JobAvailable);
        await using (var cmd = new NpgsqlCommand($"LISTEN {TaskTowerConstants.Notifications.JobAvailable}", conn))
        {
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        _logger.LogInformation("Subscribed to {Channel} channel", TaskTowerConstants.Notifications.JobAvailable);
        
        // Define the action to take when a notification is received
        conn.Notification += async (_, e) =>
        {
            // var channel = e.Channel;
            
            var parsedPayload = NotificationHelper.ParsePayload(e.Payload);
            if (!string.IsNullOrEmpty(parsedPayload.Queue) && parsedPayload.JobId != Guid.Empty)
            {
                _logger.LogDebug("Notification received for the {Queue} queue with a Job Id of {Id}", parsedPayload.Queue, parsedPayload.JobId);
                    
                // Wait to enter the semaphore before processing a job
                await _semaphore.WaitAsync(stoppingToken);
                try
                {
                    await using var notificationScope = _serviceScopeFactory.CreateAsyncScope();
                    var jobId = await MarkJobAsProcessing(notificationScope.ServiceProvider, stoppingToken);
                    if (jobId != null)
                    {
                        await ProcessAvailableJob(notificationScope.ServiceProvider, jobId.Value, stoppingToken);
                    }
                }
                finally
                {
                    // Ensure the semaphore is always released
                    _semaphore.Release();
                }
            }
        };
        
        var scheduledJobsInterval = NormalizeInterval(_options.JobCheckInterval);
        var enqueuedJobsInterval = NormalizeInterval(_options.QueueAnnouncementInterval);
        _logger.LogInformation("Polling for scheduled jobs every {PollingInterval}", scheduledJobsInterval);
        _logger.LogInformation("Polling for queue announcements every {PollingInterval}", enqueuedJobsInterval);
        var scheduledJobsTimer = new PeriodicTimer(scheduledJobsInterval);
        var enqueuedJobsTimer = new PeriodicTimer(enqueuedJobsInterval);

        _ = AnnouncingEnqueuedJobsAsync(enqueuedJobsTimer, stoppingToken);
        _ = PollingScheduledJobsAsync(scheduledJobsTimer, stoppingToken);

        // // Keep the service running until a cancellation request is received
        while (!stoppingToken.IsCancellationRequested)
        {
            // This call is blocking until a notification is received
            await conn.WaitAsync(stoppingToken);
        }
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "An error occurred while processing job notifications");
        // }
    }

    private async Task ResetMidProcessingJobs(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        _logger.LogDebug("Requeuing any jobs that stopped mid processing");
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        var processingJobs = await conn.QueryAsync<Guid>(
            $@"
SELECT id
FROM {MigrationConfig.SchemaName}.jobs
WHERE status = @Status
FOR UPDATE SKIP LOCKED",
            new { Status = JobStatus.Processing().Value },
            transaction: tx
        );
        List<Guid> processingJobsList = processingJobs?.ToList() ?? new List<Guid>();
        if (processingJobsList.Count == 0)
        {
            _logger.LogDebug("No jobs found to requeue");
            return;
        }

        await HandleProcessingHangReset(stoppingToken, processingJobsList, conn, tx, "Job was requeued after being stopped mid processing");
        _logger.LogDebug("Requeued {Count} jobs that stopped mid processing", processingJobsList.Count);
    }

    private async Task HandleProcessingHangReset(CancellationToken stoppingToken, List<Guid> processingJobsList,
        NpgsqlConnection conn, NpgsqlTransaction tx, string reason)
    {
        // for some reason the array passing is throwing -- confident of no sql injection here
        var commaSeparatedSingleQuotedJobIds = string.Join(",", processingJobsList.Select(x => $"'{x}'"));
        var sql = @$"
UPDATE {MigrationConfig.SchemaName}.jobs 
SET status = @Status 
WHERE id in ({commaSeparatedSingleQuotedJobIds})";
        await conn.ExecuteAsync(
            sql,
            new { Status = JobStatus.Enqueued().Value },
            transaction: tx
        );
        
        var runHistoriesToCreate = processingJobsList.Select(jobId => RunHistory.Create(new RunHistoryForCreation()
        {
            JobId = jobId,
            Status = RunHistoryStatus.Enqueued(),
            Comment = reason
        }))?.ToList() ?? new List<RunHistory>();
        var anonymousRunHistories = runHistoriesToCreate.Select(x => new
        {
            Id = x.Id,
            JobId = x.JobId,
            Status = x.Status.Value,
            Comment = x.Comment,
            OccurredAt = x.OccurredAt
        });
        
        await conn.ExecuteAsync(
            @$"
INSERT INTO {MigrationConfig.SchemaName}.run_histories(id, job_id, status, comment, occurred_at)
VALUES (@Id, @JobId, @Status, @Comment, @OccurredAt)",
            anonymousRunHistories,
            transaction: tx
        );

        try
        {
            await tx.CommitAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while committing transaction for requeuing jobs");

            try
            {
                await tx.RollbackAsync(stoppingToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "An error occurred during transaction rollback for requeuing jobs");
            }
        }
    }

    private TimeSpan NormalizeInterval(TimeSpan interval)
    {
        return interval <= TimeSpan.FromMilliseconds(TaskTowerConstants.Configuration.MinimumWaitIntervalMilliseconds) 
            ? TimeSpan.FromMilliseconds(TaskTowerConstants.Configuration.MinimumWaitIntervalMilliseconds) 
            : interval;
    }

    private async Task PollingScheduledJobsAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        while (await timer.WaitForNextTickAsync(stoppingToken))
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
        }
    }

    private async Task AnnouncingEnqueuedJobsAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                await AnnounceEnqueuedJobs(_options.QueuePriorities, stoppingToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
    
    private async Task AnnounceEnqueuedJobs(Dictionary<string, int> queuePriorities, CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        var enqueuedJobsList = new List<TaskTowerJob>();
        try
        {
            var enqueuedJobs = await _options.QueuePrioritization.GetEnqueuedJobs(conn, tx, queuePriorities, 8000);
            var jobsList = enqueuedJobs?.ToList();
            if (jobsList is not { Count: > 0 })
            {
                return;
            }
            _logger.LogDebug("Announcing {Count} jobs", jobsList.Count);
            enqueuedJobsList = jobsList ?? new List<TaskTowerJob>();   
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while committing transaction for getting announcable jobs");

            try
            {
                await tx.RollbackAsync(stoppingToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "An error occurred during transaction rollback for getting announcable jobs");
            }
        }
        
        foreach (var enqueuedJob in enqueuedJobsList)
        {
            await using var connAnnounce = new NpgsqlConnection(_options.ConnectionString);
            await connAnnounce.OpenAsync(stoppingToken);
            var notifyPayload = NotificationHelper.CreatePayload(enqueuedJob.Queue, enqueuedJob.Id);
            await connAnnounce.ExecuteAsync("""
                                    /* 
                                     * This query will notify the channel that a job is available for processing.
                                     */
                                    SELECT pg_notify(@Notification, @Payload);
                                    """,
                new { Notification = TaskTowerConstants.Notifications.JobAvailable, Payload = notifyPayload }
            );
            
            _logger.LogDebug("Announced job {JobId} to {Channel} channel from the queue {Queue}", 
                enqueuedJob.Id, TaskTowerConstants.Notifications.JobAvailable, enqueuedJob.Queue);
        }
    }
    
    private async Task EnqueueScheduledJobs(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);

        try
        {
            var queuePrioritization = _options.QueuePrioritization;
            var scheduledJobs = await queuePrioritization.GetJobsToEnqueue(conn, tx, 
                _options.QueuePriorities);
            var scheduledJobsList = scheduledJobs?.ToList() ?? new List<TaskTowerJob>();
            
            if (scheduledJobsList.Count == 0)
            {
                return;
            }
            
            _logger.LogDebug("Enqueuing {Count} scheduled jobs", scheduledJobsList.Count);
            var updateQuery = $"""
                                   UPDATE {MigrationConfig.SchemaName}.jobs
                                   SET status = @Status
                                   WHERE id = ANY(@JobIds);
                               """;
            await conn.ExecuteAsync(updateQuery, new 
            { 
                Status = JobStatus.Enqueued().Value, 
                JobIds = scheduledJobsList
                    .Select(j => j.Id)
                    .ToArray() 
            }, transaction: tx);
            
            var runHistories = scheduledJobsList.Select(job => new
            {
                Id = Guid.NewGuid(),
                JobId = job.Id,
                Status = JobStatus.Enqueued().Value,
                OccurredAt = DateTime.UtcNow
            }).ToList();

            var insertQuery = $"""
                                   INSERT INTO {MigrationConfig.SchemaName}.run_histories (id, job_id, status, occurred_at)
                                   VALUES {string.Join(", ", runHistories.Select((_, index) => $"(@Id{index}, @JobId{index}, @Status{index}, @OccurredAt{index})"))}
                               """;
            
            var parameters = new DynamicParameters();
            for (var i = 0; i < runHistories.Count; i++)
            {
                parameters.Add($"Id{i}", runHistories[i].Id);
                parameters.Add($"JobId{i}", runHistories[i].JobId);
                parameters.Add($"Status{i}", runHistories[i].Status);
                parameters.Add($"OccurredAt{i}", runHistories[i].OccurredAt);
            }
            await conn.ExecuteAsync(insertQuery, parameters, transaction: tx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while enqueuing scheduled jobs");
        }
        
        try
        {
            await tx.CommitAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while committing transaction for enqueuing jobs");

            try
            {
                await tx.RollbackAsync(stoppingToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "An error occurred during transaction rollback for enqueuing jobs");
            }
        }
    }

    private async Task<Guid?> MarkJobAsProcessing(IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        // TODO add connection timeout handling
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        var queuePrioritization = _options.QueuePrioritization;
        var job = await queuePrioritization.GetJobToRun(conn, tx, _options.QueuePriorities);
        
        var errorDetails = new ErrorDetails(null, null, null);
        if (job != null)
        {
            try
            {
                try
                {
                    serviceProvider = PerformPreprocessing(serviceProvider, job);
                    await MarkAsProcessing(job, conn, tx);
                }
                catch (Exception ex)
                {
                    errorDetails = await HandleFailure(job, conn, tx, ex);
                }
                finally
                {
                    await tx.CommitAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking job {JobId} from queue {Queue} with payload {Payload} as processing -- rolling back transaction", job.Id, job.Queue, job.Payload);

                try
                {
                    await tx.RollbackAsync(stoppingToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "An error occurred during transaction rollback for marking job {JobId} as processing from queue {Queue} with payload {Payload} at {Time}", job.Id, job.Queue, job.Payload, DateTimeOffset.UtcNow.ToString("o"));
                }
            }
            
            if (job.Status.IsDead())
            {
                ExecuteDeathInterceptors(serviceProvider, job, errorDetails);
            }
            
            _logger.LogDebug("Marked job {JobId} from queue {Queue} with payload {Payload} as processing at {Time}", job.Id, job.Queue, job.Payload, DateTimeOffset.UtcNow.ToString("o"));
            return job.Id;
        }
        
        return null;
    }

    private async Task ProcessAvailableJob(IServiceProvider serviceProvider, Guid jobId, CancellationToken stoppingToken)
    {
        _logger.LogDebug("Processing job {JobId} at {Time}", jobId, DateTimeOffset.UtcNow.ToString("o"));
        
        // TODO add connection timeout handling
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        var job = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
            $@"
SELECT id as Id,  
       queue as Queue, 
       status as Status, 
       type as Type, 
       method as Method, 
       parameter_types as ParameterTypes, 
       payload as Payload, 
       retries as Retries, 
       max_retries as MaxRetries, 
       run_after as RunAfter, 
       ran_at as RanAt, 
       created_at as CreatedAt, 
       deadline as Deadline,
       context_parameters as RawContextParameters
FROM {MigrationConfig.SchemaName}.jobs
WHERE id = @Id
FOR UPDATE SKIP LOCKED
LIMIT 1",
            new { Id = jobId },
            transaction: tx
        );
        
        if (job == null)
        {
            // when postgres under high load, there can be a delayed release of the lock (i.e. from updating the job to
            // `Processing` status) so we wait and then retry the selection -- increasing pool size may help
            var maxRetries = 5;
            var retryCount = 1;
            var jobProcessed = false;
    
            while (!jobProcessed && retryCount <= maxRetries)
            {
                if (retryCount > 1)
                    _logger.LogWarning(
                        "There was a delayed lock release for Job {JobId}, starting backoff and then retrying selection (retry {Retry}) -- increasing pool size may help",
                        jobId,
                        retryCount);
                
                if (retryCount == maxRetries)
                {
                    _logger.LogWarning("Job {JobId} is on it's last retry from a delayed lock release", jobId);
                }
                
                var delayDurationMs = retryCount == 0 
                    ? 1000 
                    : retryCount <= 2 
                        ? retryCount * 1000 
                        : 2500;
                if (delayDurationMs > 0)
                {
                    await Task.Delay(delayDurationMs, stoppingToken);
                }
    
                var retriedJob = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
                    $@"
    SELECT id as Id,  
           queue as Queue, 
           status as Status, 
           type as Type, 
           method as Method, 
           parameter_types as ParameterTypes, 
           payload as Payload, 
           retries as Retries, 
           max_retries as MaxRetries, 
           run_after as RunAfter, 
           ran_at as RanAt, 
           created_at as CreatedAt, 
           deadline as Deadline,
           context_parameters as RawContextParameters
    FROM {MigrationConfig.SchemaName}.jobs
    WHERE id = @Id
    FOR UPDATE SKIP LOCKED
    LIMIT 1",
                    new { Id = jobId },
                    transaction: tx
                );
    
                if (retriedJob == null)
                {
                    jobProcessed = false;
                    retryCount++;
                    continue;
                }
    
                await HandleJob(serviceProvider, stoppingToken, retriedJob, conn, tx);
                jobProcessed = true;
            }
    
            if (!jobProcessed)
            {
                _logger.LogError("Job {JobId} was not found after {MaxRetries} retries", jobId, maxRetries);
                // TODO add some kind of handling? cleanup table that can requeue?
            }
    
            return;
        }

        await HandleJob(serviceProvider, stoppingToken, job, conn, tx);
    }

    private async Task HandleJob(IServiceProvider serviceProvider, CancellationToken stoppingToken, TaskTowerJob job,
        NpgsqlConnection conn, NpgsqlTransaction tx)
    {
        _logger.LogDebug("Processing job {JobId} from queue {Queue} with payload {Payload} at {Time}", job.Id, job.Queue, job.Payload, DateTimeOffset.UtcNow.ToString("o"));
        
        var errorDetails = new ErrorDetails(null, null, null);

        try
        {
            try
            {
                await job.Invoke(serviceProvider);

                var nowDone = DateTimeOffset.UtcNow;
                await conn.ExecuteAsync(
                    $"UPDATE {MigrationConfig.SchemaName}.jobs SET status = @Status, ran_at = @Now WHERE id = @Id",
                    new { job.Id, Status = JobStatus.Completed().Value, Now = nowDone },
                    transaction: tx
                );

                var runHistory = RunHistory.Create(new RunHistoryForCreation()
                {
                    JobId = job.Id,
                    Status = RunHistoryStatus.Completed(),
                    OccurredAt = nowDone
                });
                await JobRunHistoryRepository.AddRunHistory(conn, runHistory, tx);
            }
            catch (Exception ex)
            {
                errorDetails = await HandleFailure(job, conn, tx, ex);
            }
            finally
            {
                await tx.CommitAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing job {JobId} from queue {JobQueue} with payload {JobPayload} at {Now} -- rolling back transaction", job.Id, job.Queue, job.Payload, DateTimeOffset.UtcNow.ToString("o"));

            try
            {
                await tx.RollbackAsync(stoppingToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "An error occurred during transaction rollback for job {JobId} from queue {Queue} with payload {Payload} at {Time}", job.Id, job.Queue, job.Payload, DateTimeOffset.UtcNow.ToString("o"));
            }
        }

        if (job.Status.IsDead())
        {
            ExecuteDeathInterceptors(serviceProvider, job, errorDetails);
        }
        
        _logger.LogDebug("Processed job {JobId} from queue {Queue} with payload {Payload}, finishing at {Time}", job.Id, job.Queue, job.Payload, DateTimeOffset.UtcNow.ToString("o"));
    }

    private void ExecuteDeathInterceptors(IServiceProvider serviceProvider, TaskTowerJob job, ErrorDetails errorDetails)
    {
        // TODO add interceptor span
        _logger.LogDebug("Checking for preprocessing interceptors for Job {JobId}", job.Id);
        var jobType = Type.GetType(job.Type);
        var interceptors = _options.GetInterceptors(jobType, InterceptionStage.Death());
        _logger.LogDebug("Found {Count} death interceptors for Job {JobId}", interceptors.Count, job.Id);

        if (interceptors.Count > 0)
        {
            _logger.LogDebug("Executing death interceptors for Job {JobId}", job.Id);
            foreach (var interceptor in interceptors)
            {
                serviceProvider = ExecuteInterceptor(serviceProvider, interceptor, job, errorDetails);
            }
        }
        // TODO end span
    }

    private async Task<ErrorDetails> HandleFailure(TaskTowerJob job, NpgsqlConnection conn, NpgsqlTransaction tx, Exception ex)
    {
        job.MarkAsFailed();
        await conn.ExecuteAsync(
            @$"UPDATE {MigrationConfig.SchemaName}.jobs 
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
                
        var runHistory = RunHistory.Create(new RunHistoryForCreation()
        {
            JobId = job.Id,
            Status = RunHistoryStatus.Failed(),
            Comment = ex.Message,
            Details = ex.StackTrace,
            OccurredAt = job.RanAt ?? DateTimeOffset.UtcNow
        });
        await JobRunHistoryRepository.AddRunHistory(conn, runHistory, tx);
                
        _logger.LogError("Job {JobId} failed because of {Reasons}", job.Id, ex.Message);
                
        if (job.Status.IsDead())
        {
            _logger.LogError("Job {JobId} is dead", job.Id);
        }
        return new ErrorDetails(runHistory.Comment, runHistory.Details, runHistory.OccurredAt);
    }

    private async Task MarkAsProcessing(TaskTowerJob job, NpgsqlConnection conn, NpgsqlTransaction tx)
    {
        var nowProcessing = DateTimeOffset.UtcNow;
        _logger.LogDebug("Processing job {JobId} from the {Queue} queue with a payload of {Payload} at {Now}", 
            job.Id, job.Queue, job.Payload, nowProcessing.ToString("o"));
        await conn.ExecuteAsync(
            $"UPDATE {MigrationConfig.SchemaName}.jobs SET status = @Status, ran_at = @Now WHERE id = @Id",
            new { job.Id, Status = JobStatus.Processing().Value, Now = nowProcessing },
            transaction: tx
        );
        var runHistoryProcessing = RunHistory.Create(new RunHistoryForCreation()
        {
            JobId = job.Id,
            Status = RunHistoryStatus.Processing(),
            OccurredAt = nowProcessing
        });
        await JobRunHistoryRepository.AddRunHistory(conn, runHistoryProcessing, tx);
    }

    private IServiceProvider PerformPreprocessing(IServiceProvider serviceProvider, TaskTowerJob job)
    {
        // TODO add interceptor span
        _logger.LogDebug("Checking for preprocessing interceptors for Job {JobId}", job.Id);
        var jobType = Type.GetType(job.Type);
        var interceptors = _options.GetInterceptors(jobType, InterceptionStage.PreProcessing());
        _logger.LogDebug("Found {Count} preprocessing interceptors for Job {JobId}", interceptors.Count, job.Id);

        if (interceptors.Count > 0)
        {
            _logger.LogDebug("Executing preprocessing interceptors for Job {JobId}", job.Id);
            foreach (var interceptor in interceptors)
            {
                serviceProvider = ExecuteInterceptor(serviceProvider, interceptor, job);
            }
        }
        // TODO end span
        return serviceProvider;
    }

    private IServiceProvider ExecuteInterceptor(IServiceProvider serviceProvider, Type interceptor, TaskTowerJob job, ErrorDetails? errorDetails = null!)
    {
        if (!typeof(JobInterceptor).IsAssignableFrom(interceptor))
        {
            _logger.LogWarning("Interceptor {Interceptor} is not a JobInterceptor", interceptor);
            return serviceProvider;
        }
        var jobInterceptorInstance = Activator.CreateInstance(interceptor, serviceProvider) as JobInterceptor;

        var context = JobInterceptorContext.Create(job);
        if (errorDetails != null)
        {
            context.SetErrorDetails(errorDetails);
        }
        var updatedScope = jobInterceptorInstance?.Intercept(context);

        var updatedSp = updatedScope?.GetServiceProvider();
        if (updatedSp != null)
        {
            serviceProvider = updatedSp;
        }

        return serviceProvider;
    }

    public override void Dispose()
    {
        _logger.LogWarning("Task Tower worker {Worker} is shutting down", _workerId);
        _semaphore.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}

