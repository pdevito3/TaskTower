namespace TaskTower.Exceptions;

public class InvalidTaskTowerTagException : TaskTowerException
{
    public InvalidTaskTowerTagException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public InvalidTaskTowerTagException() : base("Invalid Task Tower tag name")
    {
    }
    
    public InvalidTaskTowerTagException(string tagName) : base($"Invalid Task Tower tag name: '{tagName}'")
    {
    }
}