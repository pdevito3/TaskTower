namespace TaskTower.Domain.TaskTowerJob.Services;

using System.Text;
using Configurations;
using Dapper;
using Database;
using Microsoft.Extensions.Options;
using Npgsql;
using Resources;

public interface ITaskTowerJobRepository
{
    Task<PagedList<TaskTowerJob>> GetPaginatedJobs(int page, int pageSize, 
        string[] statuses,
        string? filterText,
        CancellationToken cancellationToken = default);
    Task AddJob(TaskTowerJob job, CancellationToken cancellationToken = default);
}

internal class TaskTowerJobRepository(IOptions<TaskTowerOptions> options) : ITaskTowerJobRepository
{
    public async Task<PagedList<TaskTowerJob>> GetPaginatedJobs(int pageNumber, 
    int pageSize,
    string[] statuses,
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

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            sqlWhereBuilder.Append(sqlWhereBuilder.Length > 0 ? "AND " : "WHERE ");
            sqlWhereBuilder.Append("""
                                   (LOWER(job_name) ILIKE(LOWER( @FilterText)) 
                                   OR LOWER(payload::text) ILIKE LOWER(@FilterText) 
                                   OR LOWER(queue) ILIKE LOWER(@FilterText) 
                                   OR LOWER(id::text) ILIKE LOWER(@FilterText)) 
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
        id as Id, 
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
    FROM {MigrationConfig.SchemaName}.jobs
    {sqlWhereBuilder}
    ORDER BY created_at DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY";
        var jobs = await conn.QueryAsync<TaskTowerJob>(sql, parameters);

        var totalJobCount = await conn.ExecuteScalarAsync<int>(@$"
    SELECT COUNT(*)
    FROM {MigrationConfig.SchemaName}.jobs
    {sqlWhereBuilder}", parameters);

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
}