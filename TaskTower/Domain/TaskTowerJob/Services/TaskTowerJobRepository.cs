namespace TaskTower.Domain.TaskTowerJob.Services;

using System.Text;
using Configurations;
using Dapper;
using Database;
using Dtos;
using Exceptions;
using JobStatuses;
using Microsoft.Extensions.Options;
using Npgsql;
using Resources;
using RunHistories;
using RunHistories.Models;
using RunHistories.Services;
using RunHistoryStatuses;

public interface ITaskTowerJobRepository
{
    Task<PagedList<TaskTowerJob>> GetPaginatedJobs(int page, int pageSize,
        string[] statuses,
        string[] queueFilter,
        string? filterText,
        CancellationToken cancellationToken = default);
    Task AddJob(TaskTowerJob job, CancellationToken cancellationToken = default);
    Task<List<string>> GetQueueNames(CancellationToken cancellationToken = default);
    Task BulkDeleteJobs(Guid[] jobIds, CancellationToken cancellationToken = default);
    Task<TaskTowerJobWithTagsView?> GetJobView(Guid jobId, CancellationToken cancellationToken = default);
    Task RequeueJob(Guid jobId, CancellationToken cancellationToken = default);
}

internal class TaskTowerJobRepository(IOptions<TaskTowerOptions> options) : ITaskTowerJobRepository
{
    public async Task<PagedList<TaskTowerJob>> GetPaginatedJobs(int pageNumber,
        int pageSize,
        string[] statuses,
        string[] queueFilter,
        string? filterText,
        CancellationToken cancellationToken = default)
    {
        var sqlWhereBuilder = new StringBuilder();
        var parameters = new DynamicParameters();

        if (statuses.Length > 0)
        {
            var lowerStatuses = statuses.Select(s => s.ToLower()).ToArray();
            sqlWhereBuilder.Append("WHERE LOWER(status) = ANY (@LowerStatuses) ");
            parameters.Add("LowerStatuses", lowerStatuses);
        }
        
        if (queueFilter.Length > 0)
        {
            var lowerQueueFilter = queueFilter.Select(s => s.ToLower()).ToArray();
            sqlWhereBuilder.Append(sqlWhereBuilder.Length > 0 ? "AND " : "WHERE ");
            sqlWhereBuilder.Append("LOWER(queue) = ANY (@LowerQueueFilter) ");
            parameters.Add("LowerQueueFilter", lowerQueueFilter);
        }

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            sqlWhereBuilder.Append(sqlWhereBuilder.Length > 0 ? "AND " : "WHERE ");
            sqlWhereBuilder.Append("""
                                   (LOWER(job_name) ILIKE LOWER(@FilterText)
                                   OR LOWER(payload::text) ILIKE LOWER(@FilterText) 
                                   OR LOWER(queue) ILIKE LOWER(@FilterText) 
                                   OR LOWER(j.id::text) ILIKE LOWER(@FilterText)
                                   OR LOWER(t.name) ILIKE LOWER(@FilterText))
                                   """);
            parameters.Add("FilterText", $"%{filterText}%");
        }

        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        var offset = (pageNumber - 1) * pageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var sql = @$"
    SELECT
        j.id as Id, 
        queue as Queue, 
        type as Type, 
        method as Method,
        parameter_types as ParameterTypes,
        payload as Payload,
        max_retries as MaxRetries,
        run_after as RunAfter,
        ran_at as RanAt,
        deadline as Deadline,
        created_at as CreatedAt,
        status as Status,
        retries as Retries,
        context_parameters as ContextParameters,
        job_name as JobName 
    FROM {MigrationConfig.SchemaName}.jobs j 
        LEFT JOIN {MigrationConfig.SchemaName}.tags t ON t.job_id = j.id
    {sqlWhereBuilder}
    GROUP BY j.id
    ORDER BY created_at DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY";
        var jobs = await conn.QueryAsync<TaskTowerJob>(sql, parameters);

        var totalSql = @$"
    SELECT COUNT(DISTINCT j.id)
    FROM {MigrationConfig.SchemaName}.jobs j
        LEFT JOIN {MigrationConfig.SchemaName}.tags t ON t.job_id = j.id
    {sqlWhereBuilder}";
        var totalJobCount = await conn.QuerySingleAsync<int>(totalSql, parameters);

        var pagedList = new PagedList<TaskTowerJob>(jobs.ToList(), totalJobCount, pageNumber, pageSize);
        return pagedList;
    }
    
    public async Task AddJob(TaskTowerJob job, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        
        await conn.ExecuteAsync(
            @$"
INSERT INTO {MigrationConfig.SchemaName}.jobs 
    (id, 
     queue, 
     type, 
     method, 
     parameter_types, 
     payload, 
     max_retries, 
     run_after, 
     ran_at, 
     deadline, 
     created_at, 
     status, 
     retries, 
     context_parameters, 
     job_name) 
VALUES (@Id, 
        @Queue, 
        @Type, 
        @Method, 
        @ParameterTypes, 
        @Payload::jsonb, 
        @MaxRetries, 
        @RunAfter, 
        @RanAt, 
        @Deadline, 
        @CreatedAt, 
        @Status, 
        @Retries, 
        @ContextParameters::jsonb, 
        @JobName)",
            new
            {
                job.Id,
                job.Queue,
                job.Type,
                job.Method,
                job.ParameterTypes,
                Payload = job.Payload,
                job.MaxRetries,
                job.RunAfter,
                job.Deadline,
                job.CreatedAt,
                Status = job.Status.Value,
                job.Retries,
                job.RanAt,
                ContextParameters = job.RawContextParameters,
                job.JobName
            });
    }
    
    public async Task<List<string>> GetQueueNames(CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        var queueNames = await conn.QueryAsync<string>(
            @$"
SELECT DISTINCT queue
FROM {MigrationConfig.SchemaName}.jobs j
ORDER BY queue");
        return queueNames.ToList();
    }
    
    public async Task BulkDeleteJobs(Guid[] jobIds, CancellationToken cancellationToken = default)
    {
        // for some reason the array passing is throwing -- confident of no sql injection here
        var commaSeparatedSingleQuotedJobIds = string.Join(",", jobIds.Select(x => $"'{x}'"));
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(
            @$"
DELETE FROM {MigrationConfig.SchemaName}.run_histories
WHERE job_id IN ({commaSeparatedSingleQuotedJobIds});

DELETE FROM {MigrationConfig.SchemaName}.tags
WHERE job_id IN ({commaSeparatedSingleQuotedJobIds});

DELETE FROM {MigrationConfig.SchemaName}.jobs
WHERE id IN ({commaSeparatedSingleQuotedJobIds});");
    }
    
    public async Task<TaskTowerJobWithTagsView?> GetJobView(Guid jobId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        var job = await conn.QueryFirstOrDefaultAsync<TaskTowerJobWithTagsView>(
            @$"
SELECT
    j.id as Id, 
    queue as Queue, 
    type as Type, 
    method as Method,
    parameter_types as ParameterTypes,
    payload as Payload,
    max_retries as MaxRetries,
    run_after as RunAfter,
    ran_at as RanAt,
    deadline as Deadline,
    created_at as CreatedAt,
    status as Status,
    retries as Retries,
    context_parameters as ContextParameters,
    job_name as JobName,
    COALESCE(STRING_AGG(t.name, ', '), null) AS TagNames
FROM {MigrationConfig.SchemaName}.jobs j
    LEFT JOIN {MigrationConfig.SchemaName}.tags t ON t.job_id = j.id
WHERE j.id = @JobId
GROUP BY j.id",
            new {JobId = jobId});
        
        return job;
    }

    public async Task RequeueJob(Guid jobId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        var job = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
            $@"
SELECT id as Id,  
       queue as Queue, 
       status as Status, 
       type as Type, 
       method as Method, 
       parameter_types as ParameterTypes, 
       payload as Payload, 
       retries as Retries, 
       max_retries as MaxRetries, 
       run_after as RunAfter, 
       ran_at as RanAt, 
       created_at as CreatedAt, 
       deadline as Deadline,
       context_parameters as RawContextParameters
FROM {MigrationConfig.SchemaName}.jobs
WHERE id = @Id
FOR UPDATE SKIP LOCKED
LIMIT 1",
            new { Id = jobId },
            transaction: tx);

        if (job == null)
        {
            throw new TaskTowerJobNotFoundException();
        }

        job.Requeue();
        var updateQuery = $"""
                               UPDATE {MigrationConfig.SchemaName}.jobs
                               SET status = @Status,
                                   run_after = @RunAfter,
                                   retries = @Retries,
                                   max_retries = @MaxRetries
                               WHERE id = @Id;
                           """;
        await conn.ExecuteAsync(updateQuery, new
        {
            Id = job.Id,
            Status = job.Status.Value,
            RunAfter = job.RunAfter,
            Retries = job.Retries,
            MaxRetries = job.MaxRetries,
        }, transaction: tx);
        
        var jobHistory = RunHistory.Create(job.Id, RunHistoryStatus.Requeued());
        await JobRunHistoryRepository.AddRunHistory(conn, jobHistory, tx);

        await tx.CommitAsync(cancellationToken);
    }
}