namespace TaskTower.Domain.QueuePrioritizations;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Ardalis.SmartEnum;
using Dapper;
using Database;
using EnqueuedJobs;
using JobStatuses;
using Npgsql;
using TaskTowerJob;

public class QueuePrioritization : ValueObject
{
    private QueuePrioritizationEnum _priority;
    public string Value
    {
        get => _priority.Name;
        private set
        {
            if (!QueuePrioritizationEnum.TryFromName(value, true, out var parsed))
                throw new ValidationException($"Invalid Queue Prioritization. Please use one of the following: {string.Join("", "", ListNames())}");

            _priority = parsed;
        }
    }
    
    public QueuePrioritization(string value)
    {
        Value = value;
    }

    public bool IsNone() => Value == None().Value;
    public static QueuePrioritization Of(string value) => new QueuePrioritization(value);
    public static implicit operator string(QueuePrioritization value) => value.Value;
    public static List<string> ListNames() => QueuePrioritizationEnum.List.Select(x => x.Name).ToList();
    public static QueuePrioritization None() => new QueuePrioritization(QueuePrioritizationEnum.None.Name);
    public static QueuePrioritization AlphaNumeric() => new QueuePrioritization(QueuePrioritizationEnum.AlphaNumeric.Name);
    public static QueuePrioritization Index() => new QueuePrioritization(QueuePrioritizationEnum.Index.Name);
    public static QueuePrioritization Weighted() => new QueuePrioritization(QueuePrioritizationEnum.Weighted.Name);
    public static QueuePrioritization Strict() => new QueuePrioritization(QueuePrioritizationEnum.Strict.Name);
    internal static QueuePrioritization FromName(string name) => new QueuePrioritization(name);
    public async Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn, 
        NpgsqlTransaction tx,
        Dictionary<string, int> queuePriorities) => await _priority.GetJobsToEnqueue(conn, tx, queuePriorities);
    public async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn,
        NpgsqlTransaction tx,
        Dictionary<string, int> queuePriorities) => await _priority.GetJobToRun(conn, tx, queuePriorities);
    public async Task<IEnumerable<TaskTowerJob>> GetEnqueuedJobs(NpgsqlConnection conn,
        Dictionary<string, int> queuePriorities,
        int limit) => await _priority.GetEnqueuedJobs(conn, queuePriorities, limit);

    protected QueuePrioritization() { } // EF Core

    private abstract class QueuePrioritizationEnum : SmartEnum<QueuePrioritizationEnum>
    {
        public static readonly QueuePrioritizationEnum None = new NoneType();
        public static readonly QueuePrioritizationEnum AlphaNumeric = new AlphaNumericType();
        public static readonly QueuePrioritizationEnum Index = new IndexType();
        public static readonly QueuePrioritizationEnum Weighted = new WeightedType();
        public static readonly QueuePrioritizationEnum Strict = new StrictType();

        protected QueuePrioritizationEnum(string name, int value) : base(name, value)
        {
        }

        public abstract Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn, 
            NpgsqlTransaction tx,
            Dictionary<string, int> queuePriorities);
        
        public abstract Task<IEnumerable<TaskTowerJob>> GetEnqueuedJobs(NpgsqlConnection conn, 
            Dictionary<string, int> queuePriorities,
            int limit);
        
        public abstract Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn, 
            NpgsqlTransaction tx,
            Dictionary<string, int> queuePriorities);

        private class NoneType : QueuePrioritizationEnum
        {
            public NoneType() : base("None", 0)
            {
            }
            
            public override async Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn, 
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                var now = DateTimeOffset.UtcNow;
                var scheduledJobs = await conn.QueryAsync<TaskTowerJob>(
                    $@"
    SELECT id, queue
    FROM {MigrationConfig.SchemaName}.jobs 
    WHERE (status = @Pending OR (status = @Failed AND retries < max_retries))
      AND run_after <= @Now
    ORDER BY run_after 
    FOR UPDATE SKIP LOCKED 
    LIMIT 1000",
                    new { Now = now, Pending = JobStatus.Pending().Value, Failed = JobStatus.Failed().Value },
                    transaction: tx
                );
                return scheduledJobs;
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetEnqueuedJobs(NpgsqlConnection conn,
                Dictionary<string, int> queuePriorities,
                int limit)
            {
                return await conn.QueryAsync<TaskTowerJob>(
                    $@"
    SELECT id, queue
    FROM {MigrationConfig.SchemaName}.jobs
    WHERE status = @Enqueued
    ORDER BY run_after 
    FOR UPDATE SKIP LOCKED
    LIMIT {limit}",
                    new { Enqueued = JobStatus.Enqueued().Value }
                );
            }

            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn,
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
                => await GetJobToRunBase(conn, tx, queuePriorities);
        }
        
        private class AlphaNumericType : QueuePrioritizationEnum
        {
            public AlphaNumericType() : base("AlphaNumeric", 1)
            {
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn,
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                var now = DateTimeOffset.UtcNow;

                var scheduledJobs = await conn.QueryAsync<TaskTowerJob>(
                    $@"
SELECT id, queue
FROM {MigrationConfig.SchemaName}.jobs 
WHERE (status = @Pending OR (status = @Failed AND retries < max_retries))
  AND run_after <= @Now
ORDER BY queue, run_after 
FOR UPDATE SKIP LOCKED 
LIMIT 1000",
                    new { Now = now, Pending = JobStatus.Pending().Value, Failed = JobStatus.Failed().Value },
                    transaction: tx
                );

                return scheduledJobs;
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetEnqueuedJobs(NpgsqlConnection conn,
                Dictionary<string, int> queuePriorities,
                int limit)
            {
                return await conn.QueryAsync<TaskTowerJob>(
                    $@"
SELECT id, queue
FROM {MigrationConfig.SchemaName}.jobs
WHERE status = @Enqueued
ORDER BY queue, run_after 
FOR UPDATE SKIP LOCKED
LIMIT {limit}",
                    new { Enqueued = JobStatus.Enqueued().Value }
                );
            }

            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn,
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
                    => await GetJobToRunBase(conn, tx, queuePriorities);
        }

        private class IndexType : QueuePrioritizationEnum
        {
            public IndexType() : base("Index", 2)
            {
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn,
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                var now = DateTimeOffset.UtcNow;
                var priorityCaseSql = string.Join(" ",
                    queuePriorities.Select((kvp, index) => $"WHEN '{kvp.Key}' THEN {kvp.Value}")
                );

                if (priorityCaseSql.Length > 0)
                {
                    priorityCaseSql = $"CASE queue {priorityCaseSql} ELSE 0 END DESC, run_after";
                }
                else
                {
                    priorityCaseSql = "run_after";
                }

                var scheduledJobs = await conn.QueryAsync<TaskTowerJob>(
                    $@"
SELECT id, queue
FROM {MigrationConfig.SchemaName}.jobs 
WHERE (status = @Pending OR (status = @Failed AND retries < max_retries))
  AND run_after <= @Now
ORDER BY {priorityCaseSql} 
FOR UPDATE SKIP LOCKED 
LIMIT 1000",
                    new { Now = now, Pending = JobStatus.Pending().Value, Failed = JobStatus.Failed().Value },
                    transaction: tx
                );

                return scheduledJobs;
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetEnqueuedJobs(NpgsqlConnection conn,
                Dictionary<string, int> queuePriorities,
                int limit)
            {
                // Construct the CASE statement for queue priorities
                var priorityCaseSql = string.Join(" ",
                    queuePriorities.Select((kvp, index) => $"WHEN '{kvp.Key}' THEN {kvp.Value}"));

                // Append the CASE statement to ORDER BY if it's not empty; otherwise, default to ordering by some other column
                if (!string.IsNullOrEmpty(priorityCaseSql))
                {
                    priorityCaseSql = $"CASE queue {priorityCaseSql} ELSE 0 END DESC, run_after";
                }
                else
                {
                    priorityCaseSql = "run_after";
                }
                
                return await conn.QueryAsync<TaskTowerJob>(
                    $@"
SELECT id, queue
FROM {MigrationConfig.SchemaName}.jobs
WHERE status = @Enqueued
ORDER BY {priorityCaseSql}
FOR UPDATE SKIP LOCKED
LIMIT {limit}",
                    new { Enqueued = JobStatus.Enqueued().Value }
                );
            }

            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn,
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
                => await GetJobToRunBase(conn, tx, queuePriorities);
        }

        private class WeightedType : QueuePrioritizationEnum
        {
            public WeightedType() : base("Weighted", 3)
            {
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn, 
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                throw new NotImplementedException();
            }
            
            public override async Task<IEnumerable<TaskTowerJob>> GetEnqueuedJobs(NpgsqlConnection conn,
                Dictionary<string, int> queuePriorities,
                int limit)
            {
                throw new NotImplementedException();
            }

            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn,
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
                => await GetJobToRunBase(conn, tx, queuePriorities);
        }
        
        private class StrictType : QueuePrioritizationEnum
        {
            public StrictType() : base("Strict", 4)
            {
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn, 
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                var priorityRanksSql = string.Join(" UNION ALL ",
                    queuePriorities.OrderBy(kvp => kvp.Value)
                        .Select(kvp => $"SELECT '{kvp.Key}' AS Priority, {kvp.Value} AS Rank"));

                var sqlQuery = $@"
WITH PriorityRanks AS (
    {priorityRanksSql}
),
HighestAvailablePriority AS (
    SELECT
        MAX(pr.Rank) AS Rank
    FROM
        {MigrationConfig.SchemaName}.jobs j
        INNER JOIN PriorityRanks pr ON j.queue = pr.Priority
    WHERE
        (j.status = @Pending OR (j.status = @Failed AND j.retries < j.max_retries))
        AND j.run_after <= @Now
)
SELECT
    j.id, j.queue
FROM
    {MigrationConfig.SchemaName}.jobs j
    INNER JOIN PriorityRanks pr ON j.queue = pr.Priority
    CROSS JOIN HighestAvailablePriority hap
WHERE
    (j.status = @Pending OR (j.status = @Failed AND j.retries < j.max_retries))
    AND j.run_after <= @Now
    AND pr.Rank = hap.Rank
ORDER BY
    j.run_after
FOR UPDATE SKIP LOCKED
LIMIT 1000;";

                var now = DateTimeOffset.UtcNow;
                var jobs = await conn.QueryAsync<TaskTowerJob>(
                    sqlQuery,
                    new { Now = now, Pending = JobStatus.Pending().Value, Failed = JobStatus.Failed().Value },
                    transaction: tx
                );
                
                return jobs;
            }

            public override async Task<IEnumerable<TaskTowerJob>> GetEnqueuedJobs(NpgsqlConnection conn, 
                Dictionary<string, int> queuePriorities,
                int limit)
            {
                var priorityRanksSql = string.Join(" UNION ALL ",
                    queuePriorities.OrderBy(kvp => kvp.Value)
                        .Select(kvp => $"SELECT '{kvp.Key}' AS Priority, {kvp.Value} AS Rank"));
                
                var sqlQuery = $@"
WITH PriorityRanks AS (
    {priorityRanksSql}
),
HighestAvailablePriority AS (
    SELECT
        MAX(pr.Rank) AS Rank
    FROM
        {MigrationConfig.SchemaName}.jobs ej
        INNER JOIN PriorityRanks pr ON ej.queue = pr.Priority
        WHERE ej.status = @Enqueued
)
SELECT ej.id, ej.queue
FROM
    {MigrationConfig.SchemaName}.jobs ej
    INNER JOIN PriorityRanks pr ON ej.queue = pr.Priority
    CROSS JOIN HighestAvailablePriority hap
WHERE pr.Rank = hap.Rank and ej.status = @Enqueued
FOR UPDATE SKIP LOCKED
LIMIT {limit};";
                
                    return await conn.QueryAsync<TaskTowerJob>(
                        sqlQuery,
                        new { Enqueued = JobStatus.Enqueued().Value }
                    );
            }

            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn, 
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
                => await GetJobToRunBase(conn, tx, queuePriorities);
        }

        private async Task<TaskTowerJob?> GetJobToRunBase(NpgsqlConnection conn, NpgsqlTransaction tx, Dictionary<string, int> queuePriorities)
        {
            var jobs = await GetEnqueuedJobs(conn, queuePriorities, 1);
            var enqueuedJob = jobs.FirstOrDefault();
                
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
                new { Id = enqueuedJob?.Id },
                transaction: tx
            );

            return job;
        }
    }
}