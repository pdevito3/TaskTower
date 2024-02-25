namespace TaskTower.Middleware;

using TaskTower.Domain.TaskTowerJob;

public class JobCreation
{
    public TaskTowerJob Job { get; private set; }
    public void SetJobContextParameter(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof (name));
        Job.SetContextParameter(name, value);
    }
    
    internal JobCreation(TaskTowerJob job)
    {
        Job = job;
    }
}

/// <summary>
/// Allows you to add context when enqueuing your job during creation that can be used by the job activator
/// </summary>
public interface IJobCreationMiddleware
{
    public void OnCreating(JobCreation context);
}