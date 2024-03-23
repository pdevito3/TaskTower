namespace TaskTower.Domain.TaskTowerJob.Features;

using Dtos;
using RunHistories.Services;
using Services;

public interface IJobViewer
{
    Task<TaskTowerJobView?> GetJobView(Guid jobId, CancellationToken cancellationToken = default);
}

internal class JobViewer(ITaskTowerJobRepository taskTowerJobRepository,
    IJobRunHistoryRepository jobRunHistoryRepository) : IJobViewer
{
    public async Task<TaskTowerJobView?> GetJobView(Guid jobId, CancellationToken cancellationToken = default)
    {
        var viewData = new TaskTowerJobView();
        var jobViewBase = await taskTowerJobRepository.GetJobView(jobId, cancellationToken);
        if (jobViewBase == null)
        {
            throw new KeyNotFoundException("Job not found");
        }
        viewData.Job = jobViewBase;
        
        var history = await jobRunHistoryRepository.GetJobRunHistoryViews(jobId, cancellationToken);
        viewData.History = history;
        
        return viewData;
    }
}