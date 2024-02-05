namespace TaskTowerSandbox.Domain.TaskTowerJob;

using JobStatuses;
using Models;

internal class TaskTowerJob
{
    internal Guid Id { get; private set; }
    
    /// <summary>
    /// An md5 hash of its queue combined with its JSON-serialized payload
    /// </summary>
    internal string? Fingerprint { get; private set; }
    
    /// <summary>
    /// The queue the job is on
    /// </summary>
    internal string Queue { get; private set; } = null!;

    /// <summary>
    /// The current status of the job
    /// </summary>
    internal JobStatus Status { get; private set; }
    
    /// <summary>
    /// JSON job payload if applicable
    /// </summary>
    internal string Payload { get; private set; } = null!;
    
    /// <summary>
    /// The number of times the job has retried
    /// </summary>
    internal int Retries { get; private set; } = 0;
    
    /// <summary>
    /// The maximum number of times the job can retry
    /// </summary>
    internal int? MaxRetries { get; private set; }
    
    /// <summary>
    /// The time after which the job is eligible to run
    /// </summary>
    internal DateTimeOffset RunAfter { get; private set; }
    
    /// <summary>
    /// The last time the job was run
    /// </summary>
    internal DateTimeOffset? RanAt { get; private set; }
    
    /// <summary>
    /// The time the job was created
    /// </summary>
    internal DateTimeOffset CreatedAt { get; private set; }
    
    /// <summary>
    /// The last error the job elicited
    /// </summary>
    internal string? Error { get; private set; }
    
    /// <summary>
    /// The time after which the job should no longer be run
    /// </summary>
    internal DateTimeOffset? Deadline { get; private set; }
    
    
    internal static TaskTowerJob Create(TaskTowerJobForCreation jobForCreation)
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
    
    internal TaskTowerJob MarkCompleted(DateTimeOffset ranAt)
    {
        Status = JobStatus.Completed();
        RanAt = ranAt;
        return this;
    }
    
    internal TaskTowerJob MarkAsFailed(string error)
    {
        Status = JobStatus.Failed();
        Error = error;
        return this;
    }
    
    internal TaskTowerJob BumpRetry()
    {
        if (Retries < MaxRetries)
            Retries++;
        return this;
    }
    
    internal TaskTowerJob ChangeRunAfter(DateTimeOffset runAfter)
    {
        RunAfter = runAfter;
        return this;
    }
    
    internal TaskTowerJob ChangeDeadline(DateTimeOffset? deadline)
    {
        Deadline = deadline;
        return this;
    }
    
    internal TaskTowerJob ChangMaxRetries(int maxRetries)
    {
        MaxRetries = maxRetries;
        return this;
    }

    private TaskTowerJob() { } // EF Core
}