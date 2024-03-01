namespace TaskTower.Configurations;

using Domain.InterceptionStages;
using Domain.QueuePrioritizations;
using Interception;

public class TaskTowerOptions
{
    /// <summary>
    /// Gets or sets the total number of backend processes available to process jobs.
    /// By default, this is set to the number of runtime CPUs available to the current process,
    /// leveraging <see cref="System.Environment.ProcessorCount"/>.
    /// </summary>
    public int BackendConcurrency { get; set; } = Environment.ProcessorCount;
    
    /// <summary>
    /// String containing connection details for the backend
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The schema to use for the task tower tables
    /// </summary>
    public string Schema { get; set; } = "task_tower";
    
    /// <summary>
    /// The interval of time between checking for new future/retry jobs that need to be enqueued
    /// </summary>
    public TimeSpan JobCheckInterval { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// The interval of time that the the queue will be scanned for new jobs to announce
    /// </summary>
    public TimeSpan QueueAnnouncementInterval { get; set; } = TimeSpan.FromSeconds(1);
    
    // TODO implement this
    /// <summary>
    /// Time duration between current time and job.RunAfter that schedule for future jobs
    /// </summary>
    public TimeSpan FutureJobWindow { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// The number of milliseconds a postgres transaction may idle before the connection is killed
    /// </summary>
    public int IdleTransactionTimeout { get; set; } = 30000;
    
    // TODO implement this
    /// <summary>
    /// Duration to wait for jobs to finish during shutdown
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; }

    // TODO implement this?
    /// <summary>
    /// Enable synchronous commits (increases durability, decreases performance) when using postgres
    /// </summary>
    public bool SynchronousCommit { get; set; } = false;
    
    // TODO add docs about setting like conStr = "Server=192.168.1.10;Port=5434;UserId=testuser;Password=1234;Database=testdb;Timeout=5;"
    // // TODO implement this?
    // /// <summary>
    // /// The amount of time to wait for a connection to become available before timing out
    // /// </summary>
    // public TimeSpan PgConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Outlines the queues that will be used along their respective priorities
    /// </summary>
    public Dictionary<string, int> QueuePriorities { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// The method of prioritizing jobs in the queue
    /// </summary>
    public QueuePrioritization QueuePrioritization { get; set; } = QueuePrioritization.None();

    public Dictionary<Type, JobConfiguration> JobConfigurations { get; private set; } = new Dictionary<Type, JobConfiguration>();
    
    /// <summary>
    /// A record of the different message types and their respective queues
    /// </summary>
    public Dictionary<Type, string> QueueAssignments
        =>  JobConfigurations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Queue);
    
    public int? GetMaxRetryCount(Type? type)
    {
        if (type != null && JobConfigurations.TryGetValue(type, out var config))
            return config.MaxRetryCount;
        
        return null;
    }
    
    public string? GetJobName(Type? type)
    {
        if (type != null && JobConfigurations.TryGetValue(type, out var config))
            return config.DisplayName;
        
        return null;
    }
    
    public List<Type> GetInterceptors(Type? type, InterceptionStage stage)
    {
        var interceptorTypes = new List<Type>();
        if(stage == InterceptionStage.PreProcessing())
        {
            var runnerContextInterceptorType = typeof(TaskTowerRunnerContextInterceptor);
            interceptorTypes.Add(runnerContextInterceptorType);
        }

        if (type != null && JobConfigurations.TryGetValue(type, out var config))
            interceptorTypes.AddRange(config.JobInterceptors
                .Where(interceptor => interceptor.Stage == stage)
                .Select(interceptor => interceptor.InterceptorType)
                .ToList());

        return interceptorTypes;
    }
    public JobConfiguration AddJobConfiguration<T>()
    {
        var config = new JobConfiguration();
        JobConfigurations[typeof(T)] = config;
        return config; // This assumes you want to directly return the JobConfiguration for chaining
    }

    
    public class JobConfiguration
    {
        public string? Queue { get; private set; }
        public string? DisplayName { get; private set; }
        public int? MaxRetryCount { get; private set; }

        public List<InterceptorAssignment> JobInterceptors { get; private set; } = new List<InterceptorAssignment>();

        // Enables fluent configuration by returning 'this'
        public JobConfiguration SetQueue(string queue)
        {
            Queue = queue;
            return this;
        }

        public JobConfiguration SetDisplayName(string displayName)
        {
            DisplayName = displayName;
            return this;
        }

        public JobConfiguration SetMaxRetryCount(int maxRetryCount)
        {
            MaxRetryCount = maxRetryCount;
            return this;
        }

        public JobConfiguration WithPreProcessingInterceptor<TJobInterceptor>() where TJobInterceptor : JobInterceptor
        {
            JobInterceptors.Add(new InterceptorAssignment(typeof(TJobInterceptor), InterceptionStage.PreProcessing()));
            return this;
        }

        public JobConfiguration WithDeathInterceptor<TJobInterceptor>() where TJobInterceptor : JobInterceptor
        {
            JobInterceptors.Add(new InterceptorAssignment(typeof(TJobInterceptor), InterceptionStage.Death()));
            return this;
        }
    }
    
    public record InterceptorAssignment(Type InterceptorType, InterceptionStage Stage);
}
