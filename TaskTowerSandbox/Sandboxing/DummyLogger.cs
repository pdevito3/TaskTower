namespace TaskTowerSandbox.Sandboxing;

public interface IDummyLogger
{
    void Log(string message);
}

public class DummyLogger : IDummyLogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
        Console.WriteLine("this was injected");
    }
}