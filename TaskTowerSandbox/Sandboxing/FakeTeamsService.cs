namespace TaskTowerSandbox.Sandboxing;

using Serilog;

public class FakeTeamsService()
{
    public void SendMessage(string channel, string message)
    {
        Log.Information("TEAMS: Sending message to the '{Channel}' channel: '{Message}'", channel, message);
    }
}
