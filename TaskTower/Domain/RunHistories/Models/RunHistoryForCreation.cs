namespace TaskTower.Domain.RunHistories.Models;

using JobStatuses;

public record RunHistoryForCreation()
{
    public Guid JobId { get;  set; }
    public JobStatus Status { get; set; }
    public string? Comment { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}