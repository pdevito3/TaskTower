namespace TaskTower.Domain.RunHistories.Services;

using System.Text;
using Configurations;
using Dapper;
using Database;
using Microsoft.Extensions.Options;
using Models;
using Npgsql;
using Resources;
using TaskTowerJob.Dtos;

internal interface IJobRunHistoryRepository
{
    Task<List<JobRunHistoryView>> GetJobRunHistoryViews(Guid jobId, CancellationToken cancellationToken = default);
}

internal class JobRunHistoryRepository(IOptions<TaskTowerOptions> options) : IJobRunHistoryRepository
{
    public async Task<List<JobRunHistoryView>> GetJobRunHistoryViews(Guid jobId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        var runHistoryView = await conn.QueryAsync<JobRunHistoryView>(
            @$"
SELECT
    rh.id as Id,
    rh.job_id as JobId,
    rh.status as Status,
    rh.comment as Comment,
    rh.details as Details,
    rh.occurred_at as OccurredAt
FROM {MigrationConfig.SchemaName}.run_histories rh
WHERE rh.job_id = @JobId
order by rh.occurred_at desc,
    CASE 
        WHEN rh.status = 'Pending' THEN 1
        WHEN rh.status = 'Enqueued' THEN 2
        WHEN rh.status = 'Processing' THEN 3
        ELSE 4
    END DESC
",
            new {JobId = jobId});
        
        return runHistoryView.ToList();
    }
}