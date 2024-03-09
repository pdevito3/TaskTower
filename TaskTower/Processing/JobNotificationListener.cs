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
    private Timer _timer = null!;

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
        var scheduledJobsTime = new PeriodicTimer(scheduledJobsInterval);
        var enqueuedJobsTimer = new PeriodicTimer(enqueuedJobsInterval);

        _ = AnnouncingEnqueuedJobsAsync(enqueuedJobsTimer, stoppingToken);
        _ = PollingScheduledJobsAsync(scheduledJobsTime, stoppingToken);

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
            Status = JobStatus.Enqueued(),
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
        
        await tx.CommitAsync(stoppingToken);
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
        
        var enqueuedJobs = await _options.QueuePrioritization.GetEnqueuedJobs(conn, tx, queuePriorities, 8000);

        foreach (var enqueuedJob in enqueuedJobs)
        {
            var notifyPayload = NotificationHelper.CreatePayload(enqueuedJob.Queue, enqueuedJob.Id);
            await conn.ExecuteAsync($"SELECT pg_notify(@Notification, @Payload)",
                new { Notification = TaskTowerConstants.Notifications.JobAvailable, Payload = notifyPayload },
                transaction: tx
            );
            
            _logger.LogDebug("Announced job {JobId} to {Channel} channel from the queue {Queue}", 
                enqueuedJob.Id, TaskTowerConstants.Notifications.JobAvailable, enqueuedJob.Queue);
        }
        
        await tx.CommitAsync(stoppingToken);
    }
    
    private async Task EnqueueScheduledJobs(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);

        var queuePrioritization = _options.QueuePrioritization;
        var scheduledJobs = await queuePrioritization.GetJobsToEnqueue(conn, tx, 
            _options.QueuePriorities);
        
        foreach (var job in scheduledJobs)
        {
            // TODO use domain model
            await conn.ExecuteAsync(
                $"UPDATE {MigrationConfig.SchemaName}.jobs SET status = @Status WHERE id = @Id",
                new { job.Id, Status = JobStatus.Enqueued().Value },
                transaction: tx
            );
            
            var runHistory = RunHistory.Create(new RunHistoryForCreation()
            {
                JobId = job.Id,
                Status = JobStatus.Enqueued()
            });
            await AddRunHistory(conn, runHistory, tx);
            
            _logger.LogDebug("Enqueued job {JobId} to the {Queue} queue", job.Id, job.Queue);
        }
    
        await tx.CommitAsync(stoppingToken);
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
        
        if(job == null)
        {
            _logger.LogDebug("Job {JobId} is still locked, waiting and then retrying selection", jobId);
            await Task.Delay(1000, stoppingToken);
            
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
            
            if(retriedJob == null)
            {
                _logger.LogDebug("Job {JobId} is still locked, can not progress", jobId);
                await HandleProcessingHangReset(stoppingToken, new List<Guid>() {jobId}, conn, tx, "Job could not be selected for processing, requeuing");                return;
            }
            
            await HandleJob(serviceProvider, stoppingToken, retriedJob, conn, tx);
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
                Status = JobStatus.Completed(),
                OccurredAt = nowDone
            });
            await AddRunHistory(conn, runHistory, tx);
        }
        catch (Exception ex)
        {
            errorDetails = await HandleFailure(job, conn, tx, ex);
        }
        finally
        {
            await tx.CommitAsync(stoppingToken);
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
            Status = JobStatus.Failed(),
            Comment = ex.Message,
            Details = ex.StackTrace,
            OccurredAt = job.RanAt ?? DateTimeOffset.UtcNow
        });
        await AddRunHistory(conn, runHistory, tx);
                
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
            Status = JobStatus.Processing(),
            OccurredAt = nowProcessing
        });
        await AddRunHistory(conn, runHistoryProcessing, tx);
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

    private static async Task AddRunHistory(NpgsqlConnection conn, RunHistory runHistory, NpgsqlTransaction tx)
    {
        await conn.ExecuteAsync(
            $"INSERT INTO {MigrationConfig.SchemaName}.run_histories(id, job_id, status, comment, details, occurred_at) VALUES (@Id, @JobId, @Status, @Comment, @Details, @OccurredAt)",
            new { runHistory.Id, runHistory.JobId, Status = runHistory.Status.Value, runHistory.Comment, runHistory.Details, runHistory.OccurredAt },
            transaction: tx
        );
    }

    public override void Dispose()
    {
        _logger.LogDebug("Task Tower worker is shutting down");
        _semaphore.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}

