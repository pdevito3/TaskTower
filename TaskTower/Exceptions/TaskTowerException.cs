namespace TaskTower.Exceptions;

public class TaskTowerException : Exception
{
    public TaskTowerException(string message) : base(message)
    {
    }

    public TaskTowerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}