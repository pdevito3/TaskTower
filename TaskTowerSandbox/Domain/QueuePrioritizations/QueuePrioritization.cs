namespace TaskTowerSandbox.Domain.QueuePrioritizationes;

using System.ComponentModel.DataAnnotations;
using Ardalis.SmartEnum;
using Dapper;
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
    public static QueuePrioritization Strict() => new QueuePrioritization(QueuePrioritizationEnum.Strict.Name);
    public static QueuePrioritization Weighted() => new QueuePrioritization(QueuePrioritizationEnum.Weighted.Name);
    internal static QueuePrioritization FromName(string name) => new QueuePrioritization(name);
    public async Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn, 
        NpgsqlTransaction tx,
        Dictionary<string, int> queuePriorities) => await _priority.GetJobsToEnqueue(conn, tx, queuePriorities);
    public async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn,
        NpgsqlTransaction tx,
        Dictionary<string, int> queuePriorities) => await _priority.GetJobToRun(conn, tx, queuePriorities);

    protected QueuePrioritization() { } // EF Core

    private abstract class QueuePrioritizationEnum : SmartEnum<QueuePrioritizationEnum>
    {
        public static readonly QueuePrioritizationEnum None = new NoneType();
        public static readonly QueuePrioritizationEnum Strict = new StrictType();
        public static readonly QueuePrioritizationEnum Weighted = new WeightedType();

        protected QueuePrioritizationEnum(string name, int value) : base(name, value)
        {
        }

        public abstract Task<IEnumerable<TaskTowerJob>> GetJobsToEnqueue(NpgsqlConnection conn, 
            NpgsqlTransaction tx,
            Dictionary<string, int> queuePriorities);
        
        // get job to run
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
    FROM jobs 
    WHERE (status = @Pending OR (status = @Failed AND retries < max_retries))
      AND run_after <= @Now
    ORDER BY run_after 
    FOR UPDATE SKIP LOCKED 
    LIMIT 8000",
                    new { Now = now, Pending = JobStatus.Pending().Value, Failed = JobStatus.Failed().Value },
                    transaction: tx
                );
                return scheduledJobs;
            }
            
            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn, 
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                var enqueuedJob = await conn.QueryFirstOrDefaultAsync<EnqueuedJob>(
                    $@"
    SELECT job_id as JobId, queue as Queue
    FROM enqueued_jobs
    FOR UPDATE SKIP LOCKED
    LIMIT 1",
                    transaction: tx
                );
                
                var job = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
                    $@"
SELECT id, queue, payload, retries
FROM jobs
WHERE id = @Id
FOR UPDATE SKIP LOCKED
LIMIT 1",
                    new { Id = enqueuedJob?.JobId },
                    transaction: tx
                );
                
                return job;
            }
        }

        private class StrictType : QueuePrioritizationEnum
        {
            public StrictType() : base("Strict", 2)
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
                    priorityCaseSql = $"CASE queue {priorityCaseSql} ELSE 0 END DESC,";
                }

                var scheduledJobs = await conn.QueryAsync<TaskTowerJob>(
                    $@"
SELECT id, queue
FROM jobs 
WHERE (status = @Pending OR (status = @Failed AND retries < max_retries))
  AND run_after <= @Now
ORDER BY {priorityCaseSql} run_after 
FOR UPDATE SKIP LOCKED 
LIMIT 8000",
                    new { Now = now, Pending = JobStatus.Pending().Value, Failed = JobStatus.Failed().Value },
                    transaction: tx
                );

                return scheduledJobs;
            }

            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn,
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                // Construct the CASE statement for queue priorities
                var priorityCaseSql = string.Join(" ",
                    queuePriorities.Select((kvp, index) => $"WHEN '{kvp.Key}' THEN {kvp.Value}"));

                // Append the CASE statement to ORDER BY if it's not empty; otherwise, default to ordering by some other column
                if (!string.IsNullOrEmpty(priorityCaseSql))
                {
                    priorityCaseSql = $"CASE queue {priorityCaseSql} ELSE 0 END";
                }
                else
                {
                    // Fallback ordering, e.g., by id or run_after, if no priority case statement is constructed
                    priorityCaseSql =
                        "id"; // Adjust this line according to your table's structure and desired default ordering
                }

                var sql = $@"
SELECT job_id as JobId, queue as Queue
FROM enqueued_jobs
ORDER BY {priorityCaseSql} DESC
FOR UPDATE SKIP LOCKED
LIMIT 1";
                var enqueuedJob = await conn.QueryFirstOrDefaultAsync<EnqueuedJob>(
                    $@"
SELECT job_id as JobId, queue as Queue
FROM enqueued_jobs
ORDER BY {priorityCaseSql} DESC
FOR UPDATE SKIP LOCKED
LIMIT 1",
                    transaction: tx
                );
                
                var job = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
                    $@"
SELECT id, queue, payload, retries
FROM jobs
WHERE id = @Id
FOR UPDATE SKIP LOCKED
LIMIT 1",
                    new { Id = enqueuedJob?.JobId },
                    transaction: tx
                );

                return job;
            }
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
            
            public override async Task<TaskTowerJob?> GetJobToRun(NpgsqlConnection conn, 
                NpgsqlTransaction tx,
                Dictionary<string, int> queuePriorities)
            {
                throw new NotImplementedException();
            }
        }
    }
}