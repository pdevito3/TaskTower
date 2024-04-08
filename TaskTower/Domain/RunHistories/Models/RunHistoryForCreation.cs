namespace TaskTower.Domain.RunHistories.Models;

using JobStatuses;
using RunHistoryStatuses;

public record RunHistoryForCreation()
{
    public Guid JobId { get;  set; }
    public RunHistoryStatus Status { get; set; }
    public string? Comment { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}