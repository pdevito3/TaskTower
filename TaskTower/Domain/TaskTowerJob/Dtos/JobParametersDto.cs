namespace TaskTower.Domain.TaskTowerJob.Dtos;

using Resources;

public class JobParametersDto : BasePaginationParameters
{
    public string[] StatusFilter { get; set; } = Array.Empty<string>();
    public string[] QueueFilter { get; set; } = Array.Empty<string>();
    public string? FilterText { get; set; }
}