namespace TaskTowerSandbox.Sandboxing;

using Serilog;

public class DoAThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        await Task.Delay(1000);
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}

public class DoAPossiblyFailingThing
{
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        var success = new Random().Next(0, 100) < 70;
        if (!success)
        {
            throw new Exception("Failed");
        }
        
        Log.Information("Handled DoAPossiblyFailingThing with data: {Data}", request.Data);
        if (request.Data == "fail")
        {
            throw new Exception("Failed");
        }
    }
}