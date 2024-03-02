namespace TaskTower.Domain.TaskTowerJob.Services;

using Configurations;
using Dapper;
using Database;
using Microsoft.Extensions.Options;
using Npgsql;

public interface ITaskTowerJobRepository
{
    Task<IEnumerable<TaskTowerJob>> GetJobs(CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskTowerJob>> GetPaginatedJobs(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddJob(TaskTowerJob job, CancellationToken cancellationToken = default);
}

public class TaskTowerJobRepository(IOptions<TaskTowerOptions> options) : ITaskTowerJobRepository
{
    public async Task<IEnumerable<TaskTowerJob>> GetJobs(CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        
        var jobs = await conn.QueryAsync<TaskTowerJob>(@$"
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
FROM {MigrationConfig.SchemaName}.jobs", cancellationToken);
        
        return jobs;
    }
    public async Task<IEnumerable<TaskTowerJob>> GetPaginatedJobs(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        var offset = (page - 1) * pageSize;
        
        var jobs = await conn.QueryAsync<TaskTowerJob>(@$"
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
ORDER BY created_at
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY", 
            new { Offset = offset, PageSize = pageSize });
        
        return jobs;
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