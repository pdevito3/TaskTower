namespace TaskTower.Domain.RunHistories;

using JobStatuses;
using Models;
using RunHistoryStatuses;
using TaskTowerJob;

public class RunHistory
{
    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    internal TaskTowerJob Job { get; } = null!;
    public RunHistoryStatus Status { get; private set; } = default!;
    public string? Comment { get; private set; }
    public string? Details { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    
    public static RunHistory Create(RunHistoryForCreation runHistoryForCreation)
    {
        var runHistory = new RunHistory();
        
        runHistory.Id = Guid.NewGuid();
        runHistory.JobId = runHistoryForCreation.JobId;
        runHistory.Status = runHistoryForCreation.Status;
        runHistory.Comment = runHistoryForCreation.Comment;
        runHistory.Details = runHistoryForCreation.Details;
        runHistory.OccurredAt = runHistoryForCreation.OccurredAt ?? DateTimeOffset.UtcNow;
        return runHistory;
    }
    
    public static RunHistory Create(Guid jobId, RunHistoryStatus status)
    {
        var historyForCreation = new RunHistoryForCreation()
        {
            JobId = jobId,
            Status = status
        };
        return Create(historyForCreation);
    }
}