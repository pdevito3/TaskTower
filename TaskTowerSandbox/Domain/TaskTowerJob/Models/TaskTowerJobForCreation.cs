namespace TaskTowerSandbox.Domain.TaskTowerJob.Models;

public record TaskTowerJobForCreation()
{
    public string? Queue { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Method { get; set; } = null!;
    public string[]? ParameterTypes { get; set; }
    public string Payload { get; set; } = null!;
    public int? MaxRetries { get; set; }
    public DateTimeOffset? RunAfter { get; set; }
    public DateTimeOffset? Deadline { get; set; }
}