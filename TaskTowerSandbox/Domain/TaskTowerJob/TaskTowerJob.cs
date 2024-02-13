namespace TaskTowerSandbox.Domain.TaskTowerJob;

using EnqueuedJobs;
using JobStatuses;
using Models;

public class TaskTowerJob
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// An md5 hash of its queue combined with its JSON-serialized payload
    /// </summary>
    public string? Fingerprint { get; private set; }
    
    /// <summary>
    /// The queue the job is on
    /// </summary>
    public string Queue { get; private set; } = null!;

    /// <summary>
    /// The current status of the job
    /// </summary>
    public JobStatus Status { get; private set; }
    
    /// <summary>
    /// JSON job payload if applicable
    /// </summary>
    public string Payload { get; private set; } = null!;
    
    /// <summary>
    /// The number of times the job has retried
    /// </summary>
    public int Retries { get; private set; } = 0;
    
    /// <summary>
    /// The maximum number of times the job can retry
    /// </summary>
    public int? MaxRetries { get; private set; }
    
    /// <summary>
    /// The time after which the job is eligible to run
    /// </summary>
    public DateTimeOffset RunAfter { get; private set; }
    
    /// <summary>
    /// The last time the job was run
    /// </summary>
    public DateTimeOffset? RanAt { get; private set; }
    
    /// <summary>
    /// The time the job was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }
    
    /// <summary>
    /// The last error the job elicited
    /// </summary>
    public string? Error { get; private set; }
    
    /// <summary>
    /// The time after which the job should no longer be run
    /// </summary>
    public DateTimeOffset? Deadline { get; private set; }

    internal EnqueuedJob? EnqueuedJob { get; } = null!;


    public static TaskTowerJob Create(TaskTowerJobForCreation jobForCreation)
    {
        var TaskTowerJob = new TaskTowerJob();

        // TODO  default queue?
        // TODO  default retries?
        // TODO  default max retries?
        
        TaskTowerJob.Status = JobStatus.Pending();
        TaskTowerJob.Retries = jobForCreation.Retries;
        TaskTowerJob.MaxRetries = jobForCreation.MaxRetries ?? 20;
        TaskTowerJob.RunAfter = jobForCreation.RunAfter ?? DateTimeOffset.UtcNow;
        TaskTowerJob.Deadline = jobForCreation.Deadline;
        TaskTowerJob.CreatedAt = DateTimeOffset.UtcNow;
        
        TaskTowerJob.Payload = jobForCreation.Payload;
        TaskTowerJob.Queue = jobForCreation.Queue;
        TaskTowerJob.FingerprintJob();

        // TaskTowerJob.QueueDomainEvent(new TaskTowerJobCreated(){ TaskTowerJob = TaskTowerJob });
        
        return TaskTowerJob;
    }
    
    private void FingerprintJob()
    {
        if (Fingerprint != null)
            return;

        var js = System.Text.Json.JsonSerializer.Serialize(Payload);
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(Queue + js));
        Fingerprint = BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
    
    public TaskTowerJob MarkCompleted(DateTimeOffset ranAt)
    {
        Status = JobStatus.Completed();
        RanAt = ranAt;
        return this;
    }
    
    public TaskTowerJob MarkAsFailed(string error)
    {
        Status = JobStatus.Failed();
        Error = error;
        return this;
    }
    
    public TaskTowerJob BumpRetry()
    {
        if (Retries < MaxRetries)
            Retries++;
        return this;
    }
    
    public TaskTowerJob ChangeRunAfter(DateTimeOffset runAfter)
    {
        RunAfter = runAfter;
        return this;
    }
    
    public TaskTowerJob ChangeDeadline(DateTimeOffset? deadline)
    {
        Deadline = deadline;
        return this;
    }
    
    public TaskTowerJob ChangMaxRetries(int maxRetries)
    {
        MaxRetries = maxRetries;
        return this;
    }

    private TaskTowerJob() { } // EF Core
}