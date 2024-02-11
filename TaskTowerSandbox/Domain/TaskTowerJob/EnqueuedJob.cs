namespace TaskTowerSandbox.Domain.TaskTowerJob;

using JobStatuses;
using Models;

public class EnqueuedJob
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// The queue to enqueue the job to
    /// </summary>
    public string Queue { get; private set; } = null!;

    /// <summary>
    /// The id of the job
    /// </summary>
    public Guid JobId { get; private set; }
    internal TaskTowerJob Job { get; } = null!;
    
    public static EnqueuedJob Create(string queue, Guid jobId)
    {
        var enqueuedJob = new EnqueuedJob();
        enqueuedJob.Queue = queue;
        enqueuedJob.JobId = jobId;
        return enqueuedJob;
    }

    private EnqueuedJob() { } // EF Core
}