namespace TaskTowerSandbox.Domain.TaskTowerJob.Models;

public record TaskTowerJobForCreation()
{
    public string Queue { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public int Retries { get; set; }
    public int? MaxRetries { get; set; }
    public DateTimeOffset? RunAfter { get; set; }
    public DateTimeOffset? Deadline { get; set; }
}