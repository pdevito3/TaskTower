namespace TaskTowerSandbox.Domain.RunHistories;

using JobStatuses;
using Models;

public class RunHistory
{
    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public JobStatus Status { get; private set; } = default!;
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
        runHistory.OccurredAt = DateTimeOffset.UtcNow;
        return runHistory;
    }
}