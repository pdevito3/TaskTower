namespace TaskTower.Exceptions;

public class TaskTowerJobNotFoundException : TaskTowerException
{
    public TaskTowerJobNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public TaskTowerJobNotFoundException() : base("Task Tower job not found")
    {
    }
}