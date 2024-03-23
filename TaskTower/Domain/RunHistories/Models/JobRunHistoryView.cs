namespace TaskTower.Domain.RunHistories.Models;

using JobStatuses;

public record JobRunHistoryView()
{
    public Guid Id { get; set; }
    public Guid JobId { get;  set; }
    public string Status { get; set; } = null!;
    public string? Comment { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}