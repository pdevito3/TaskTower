namespace TaskTower.Domain.TaskTowerJob.Dtos;

using RunHistories.Models;

public sealed record TaskTowerJobView
{
    public TaskTowerJobWithTagsView Job { get; set; } = null!;
    public List<JobRunHistoryView> History { get; set; } = new();
}