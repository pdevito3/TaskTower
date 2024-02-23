namespace TaskTower.Utils;


public static class NotificationHelper
{
    public static string CreatePayload(string queue, Guid jobId) => $"Queue: {queue}, ID: {jobId}";

    public static (string Queue, Guid JobId) ParsePayload(string payload)
    {
        var parts = payload.Split(new[] { ", ID: " }, StringSplitOptions.None);
        var queuePart = parts[0].Substring("Queue: ".Length);
        var idPart = parts.Length > 1 ? Guid.Parse(parts[1]) : Guid.Empty;

        return (queuePart, idPart);
    }
}