namespace TaskTowerSandbox.Configurations;

using Domain.QueuePrioritizationes;

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
    /// The interval of time between checking for new future/retry jobs
    /// </summary>
    public TimeSpan JobCheckInterval { get; set; } = TimeSpan.FromSeconds(1);
    
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
}
