namespace RecipeManagement.Extensions;

using System.Data;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Databases;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Resources;
using Serilog;

public class JobPollingService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public JobPollingService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     var numberOfWorkers = 15;
    //     var tasks = new List<Task>();
    //
    //     for (int i = 0; i < numberOfWorkers; i++)
    //     {
    //         tasks.Add(Task.Run(async () =>
    //         {
    //             while (!stoppingToken.IsCancellationRequested)
    //             {
    //                 using (var scope = _serviceScopeFactory.CreateScope())
    //                 {
    //                     var jobExecutor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
    //                     var context = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
    //
    //                     await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
    //                     {
    //                         await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, stoppingToken);
    //                         const int batchSize = 1000;
    //                         var jobsToExecute = context.Jobs
    //                             .Where(job => job.State == JobState.Enqueued)
    //                             .TagWith(Consts.RowLockTag)
    //                             .OrderBy(job => job.CreatedAt)
    //                             .Take(batchSize) // this batch limits my throughout based on polling
    //                             // (e.g can't do more than ## messages every xx polling seconds)
    //                             .ToList();
    //
    //                         foreach (var job in jobsToExecute)
    //                         {
    //                             try
    //                             {
    //                                 jobExecutor.ExecuteJob(job.Id);
    //                                 // await context.SaveChangesAsync(stoppingToken);
    //                                 await transaction.CommitAsync(stoppingToken);
    //                                 // Log.Information($"Job {job.Id} executed successfully.");
    //                             }
    //                             catch (Exception ex)
    //                             {
    //                                 // Only roll back if the transaction hasn't been completed yet
    //                                 if (transaction.GetDbTransaction().Connection != null)
    //                                 {
    //                                     await transaction.RollbackAsync(stoppingToken);
    //                                 }
    //
    //                                 Log.Error($"Error executing job {job.Id}: {ex.Message}");
    //                                 // Consider whether to continue processing other jobs after a failure
    //                             }
    //                         }
    //                     });
    //                 }
    //
    //                 await Task.Delay(500, stoppingToken);
    //             }
    //         
    //         }, stoppingToken));
    //     }
    //
    //     // Wait for all tasks to complete or stop if cancellation is requested
    //     await Task.WhenAll(tasks);
    // }

    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     while (!stoppingToken.IsCancellationRequested)
    //     {
    //         using (var scope = _serviceScopeFactory.CreateScope())
    //         {
    //             var jobExecutor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
    //             var context = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
    //
    //             // TODO need transaction before the select...?
    //             const int batchSize = 1000;
    //             var jobsToExecute = context.Jobs
    //                 .Where(job => job.State == JobState.Enqueued)
    //                 .TagWith(Consts.RowLockTag)
    //                 .OrderBy(job => job.CreatedAt)
    //                 .Take(batchSize) // this batch limits my throughout based on polling
    //                 // (e.g can't do more than ## messages every xx polling seconds)
    //                 .ToList();
    //
    //             foreach (var job in jobsToExecute)
    //             {
    //                 await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);
    //                 try
    //                 {
    //                     await jobExecutor.ExecuteJob(job.Id, stoppingToken);
    //                     await transaction.CommitAsync(stoppingToken);
    //                     // Log.Information($"Job {job.Id} executed successfully.");
    //                 }
    //                 catch (Exception ex)
    //                 {
    //                     // Only roll back if the transaction hasn't been completed yet
    //                     if (transaction.GetDbTransaction().Connection != null)
    //                     {
    //                         await transaction.RollbackAsync(stoppingToken);
    //                     }
    //
    //                     Log.Error($"Error executing job {job.Id}: {ex.Message}");
    //                     // Consider whether to continue processing other jobs after a failure
    //                 }
    //             }
    //         }
    //
    //         await Task.Delay(500, stoppingToken);
    //     }
    // }
    
    
    // https://chat.openai.com/share/4981f801-1a89-442e-a893-f8211c86e2e8
    // To add concurrency support to your custom task runner, you will need to implement a mechanism that allows multiple jobs to be processed in parallel without stepping on each other's toes. Here are several aspects you can consider:
    // 
    // Task Parallel Library (TPL): Use the TPL to run multiple jobs concurrently within the same process. You can use Task.Run or Parallel.ForEach to process multiple jobs in parallel. However, be cautious with the number of concurrent tasks as it can overwhelm your system resources.
    // 
    // Async-Await: Since you're already using async-await, make sure that IJobExecutor.ExecuteJob is an asynchronous method and awaits on any IO-bound work. This will help in utilizing the thread-pool efficiently.
    // 
    // Partitioning: In a multi-threaded environment, partition the jobs so that each thread or task processes a subset of jobs, reducing contention.
    // 
    // Database Concurrency: Ensure that once a job is being processed, it is not fetched by another thread for processing. This usually involves setting the job state to something like Processing and committing the transaction before the actual job execution begins.
    // 
    // Transaction Scope: Start the transaction before fetching the jobs to execute. This will lock the records in the database and prevent other instances from picking them up.
    // 
    // Distributed Locking: If you have multiple instances of the job runner, you might need a distributed locking mechanism like RedLock algorithm with Redis or utilizing database row-level locking features.
    // 
    // Polling Interval: Dynamic polling intervals or a back-off strategy can help manage the load on the database and the job system more efficiently.
    // 
    // Scalability: Consider breaking the job executor out into a separate service that you can scale out. This would allow you to have multiple instances of the executor service processing jobs concurrently.
    // 
    // Message Queues: To avoid polling, consider using a message queue like RabbitMQ or Azure Service Bus. They have built-in support for concurrent processing and can deliver jobs to multiple worker instances.
    // 
    // Error Handling: Implement robust error handling and make sure to catch exceptions inside the job execution loop to prevent one failed job from stopping the processing of others.
    private const int ConcurrencyLevel = 100;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var tasks = new List<Task>();
            var jobsFound = 0;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
                
                // Start transaction before fetching jobs
                await using (var transaction = await context.Database.BeginTransactionAsync(stoppingToken))
                {
                    var jobsToExecute = context.Jobs
                        .Where(job => job.State == JobState.Enqueued)
                        .TagWith(Consts.RowLockTag)
                        .OrderBy(job => job.CreatedAt)
                        .Take(ConcurrencyLevel)
                        .ToList();
    
                    if (jobsToExecute.Count > 0)
                    {
                        jobsFound = jobsToExecute.Count; 
                        // Update job state to prevent other instances from picking it up
                        jobsToExecute.ForEach(job => job.State = JobState.Processing);
                        await transaction.CommitAsync(stoppingToken);
            
                        foreach (var job in jobsToExecute)
                        {
                            tasks.Add(RunJobAsync(job, stoppingToken));
                        }
                    }
                }
                await Task.WhenAll(tasks);
            }
        
            var backOffDelay = CalculateBackOffDelay(jobsFound);
            await Task.Delay(backOffDelay, stoppingToken);
        }
    }
    
    private const int BaseDelayMilliseconds = 500;
    private const int MaxDelayMilliseconds = 30000;
    private int _emptyPolls = 0;
    
    private TimeSpan CalculateBackOffDelay(int jobsFound)
    {
        if (jobsFound > 0)
        {
            _emptyPolls = 0;
            return TimeSpan.FromMilliseconds(BaseDelayMilliseconds);
        }
    
        _emptyPolls++;
        var delay = (int)Math.Min(
            BaseDelayMilliseconds * Math.Pow(2, _emptyPolls),
            MaxDelayMilliseconds
        );
        return TimeSpan.FromMilliseconds(delay);
    }
    
    
    private async Task RunJobAsync(Job job, CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var jobExecutor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
        var context = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
    
        await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);
        try
        {
            await jobExecutor.ExecuteJob(job.Id, stoppingToken);
    
            job.State = JobState.Succeeded;
            await transaction.CommitAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(stoppingToken);
            Log.Error($"Error executing job {job.Id}: {ex.Message}");
    
            job.State = JobState.Failed;
            await context.SaveChangesAsync(stoppingToken);
        }
    }
    
    // private const int ConcurrencyLevel = 100;
    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     using var scope = _serviceScopeFactory.CreateScope();
    //     var context = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
    //     await using var conn = new NpgsqlConnection(context.Database.GetConnectionString());
    //     await conn.OpenAsync(stoppingToken);
    //     await using var cmd = new NpgsqlCommand("LISTEN new_job", conn);
    //     await cmd.ExecuteNonQueryAsync(stoppingToken);
    //
    //     conn.Notification += async (o, e) =>
    //     {
    //         await ProcessJobsAsync("notification", stoppingToken);
    //     };
    //
    //     // Wait for notifications, but also periodically check for available jobs
    //     // in case a notification was missed
    //     while (!stoppingToken.IsCancellationRequested)
    //     {
    //         await conn.WaitAsync(stoppingToken);
    //         await ProcessJobsAsync("poll", stoppingToken); // Fallback processing
    //         await Task.Delay(10000, stoppingToken); // Fallback delay
    //     }
    // }
    //
    // private async Task ProcessJobsAsync(string callingContext, CancellationToken stoppingToken)
    // {
    //     Log.Information($"Called from {callingContext} context");
    //     var tasks = new List<Task>();
    //     using var scope = _serviceScopeFactory.CreateScope();
    //     var context = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
    //
    //     await using (var transaction = await context.Database.BeginTransactionAsync(stoppingToken))
    //     {
    //         var jobsToExecute = context.Jobs
    //             .Where(job => job.State == JobState.Enqueued)
    //             .TagWith(Consts.RowLockTag)
    //             .OrderBy(job => job.CreatedAt)
    //             .Take(ConcurrencyLevel)
    //             .ToList();
    //
    //         if (jobsToExecute.Count > 0)
    //         {
    //             jobsToExecute.ForEach(job => job.State = JobState.Processing);
    //             await transaction.CommitAsync(stoppingToken);
    //
    //             foreach (var job in jobsToExecute)
    //             {
    //                 tasks.Add(RunJobAsync(job, stoppingToken));
    //             }
    //         }
    //     }
    //     await Task.WhenAll(tasks);
    // }
}
    
