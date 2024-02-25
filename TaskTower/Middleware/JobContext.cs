namespace TaskTower.Middleware;

using TaskTower.Domain.TaskTowerJob;

public class JobContext
{
    public TaskTowerJob Job { get; }
    public void SetJobContextParameter(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof (name));
        Job.SetContextParameter(name, value);
    }
    
    internal JobContext(TaskTowerJob job)
    {
        Job = job;
    }
}

/// <summary>
/// Allows you to add context when creating your job that can be used later in the job's lifecycle (e.g. interceptors).
/// </summary>
public interface IJobContextualizer
{
    public void EnrichContext(JobContext context);
}