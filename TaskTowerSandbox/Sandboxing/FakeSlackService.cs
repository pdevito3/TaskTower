namespace TaskTowerSandbox.Sandboxing;

using Serilog;

public class FakeSlackService()
{
    public void SendMessage(string channel, string message)
    {
        Log.Information("SLACK: Sending message to the '{Channel}' channel: '{Message}'", channel, message);
    }
}
