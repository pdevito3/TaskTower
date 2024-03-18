namespace TaskTower.Exceptions;

public class MissingTaskTowerOptionsException : TaskTowerException
{
    public MissingTaskTowerOptionsException(string message) : base(message)
    {
    }
    
    public MissingTaskTowerOptionsException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public MissingTaskTowerOptionsException() : base("No TaskTowerOptions were found in the service provider")
    {
    }
}