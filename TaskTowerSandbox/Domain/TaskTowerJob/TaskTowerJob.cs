namespace TaskTowerSandbox.Domain.TaskTowerJob;

using System.Text.Json;
using EnqueuedJobs;
using JobStatuses;
using Models;
using RunHistories;

public class TaskTowerJob
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// An md5 hash of its queue combined with its JSON-serialized payload
    /// </summary>
    public string? Fingerprint { get; private set; }
    
    /// <summary>
    /// The queue the job is on
    /// </summary>
    public string? Queue { get; private set; }

    /// <summary>
    /// The current status of the job
    /// </summary>
    public JobStatus Status { get; private set; }
    
    /// <summary>
    /// Fully qualified type name
    /// </summary>
    public string Type { get; private set; } = null!;
    
    /// <summary>
    /// Method name to invoke
    /// </summary>
    public string Method { get; private set; } = null!;

    /// <summary>
    /// List of fully qualified type names for parameters
    /// </summary>
    public string[] ParameterTypes { get; private set; } = Array.Empty<string>();
    
    /// <summary>
    /// JSON job payload if applicable
    /// </summary>
    public string Payload { get; private set; } = null!;
    
    /// <summary>
    /// The number of times the job has retried
    /// </summary>
    public int Retries { get; private set; } = 0;
    
    /// <summary>
    /// The maximum number of times the job can retry
    /// </summary>
    public int? MaxRetries { get; private set; }
    
    /// <summary>
    /// The time after which the job is eligible to run
    /// </summary>
    public DateTimeOffset RunAfter { get; private set; }
    
    /// <summary>
    /// The last time the job was run
    /// </summary>
    public DateTimeOffset? RanAt { get; private set; }
    
    /// <summary>
    /// The time the job was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }
    
    /// <summary>
    /// The time after which the job should no longer be run
    /// </summary>
    public DateTimeOffset? Deadline { get; private set; }

    internal EnqueuedJob? EnqueuedJob { get; } = null!;
    
    private readonly List<RunHistory> _runHistory = new();
    internal IReadOnlyCollection<RunHistory> RunHistory => _runHistory.AsReadOnly();


    public static TaskTowerJob Create(TaskTowerJobForCreation jobForCreation)
    {
        var TaskTowerJob = new TaskTowerJob();
        
        TaskTowerJob.Status = JobStatus.Pending();
        TaskTowerJob.Retries = 0;
        TaskTowerJob.MaxRetries = jobForCreation.MaxRetries ?? 20;
        TaskTowerJob.RunAfter = jobForCreation.RunAfter ?? DateTimeOffset.UtcNow;
        TaskTowerJob.Deadline = jobForCreation.Deadline;
        TaskTowerJob.CreatedAt = DateTimeOffset.UtcNow;
        
        TaskTowerJob.Type = jobForCreation.Type;
        TaskTowerJob.Method = jobForCreation.Method;
        TaskTowerJob.ParameterTypes = jobForCreation.ParameterTypes ?? Array.Empty<string>();
        TaskTowerJob.Payload = jobForCreation.Payload;
        TaskTowerJob.Queue = jobForCreation.Queue;
        TaskTowerJob.FingerprintJob();

        // TaskTowerJob.QueueDomainEvent(new TaskTowerJobCreated(){ TaskTowerJob = TaskTowerJob });
        
        return TaskTowerJob;
    }
    
    // public async Task Invoke()
    // {
    //     if (Status.IsPending())
    //         Status = JobStatus.Enqueued();
    //     
    //     if (Status.IsEnqueued())
    //         Status = JobStatus.Processing();
    //     
    //     if (Status.IsProcessing())
    //     {
    //         var runHistory = new RunHistory();
    //         _runHistory.Add(runHistory);
    //         // QueueDomainEvent(new TaskTowerJobProcessing(){ TaskTowerJob = this });
    //         
    //         try
    //         {
    //             // var handler = _serviceProvider.GetRequiredService(Type.GetType(Type));
    //             // var method = handler.GetType().GetMethod(Method);
    //             // var parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();
    //             // var arguments = JsonSerializer.Deserialize(Payload, parameters);
    //             // var result = await (Task)method.Invoke(handler, arguments);
    //             // runHistory.MarkCompleted();
    //             // QueueDomainEvent(new TaskTowerJobCompleted(){ TaskTowerJob = this });
    //         }
    //         catch (Exception ex)
    //         {
    //             runHistory.MarkAsFailed(ex.Message);
    //             // QueueDomainEvent(new TaskTowerJobFailed(){ TaskTowerJob = this });
    //         }
    //     }
    // }
    
    public async Task Invoke()
    {
        var handlerType = System.Type.GetType(Type);
        if (handlerType == null) throw new InvalidOperationException($"Handler type '{Type}' not found.");

        var method = handlerType.GetMethod(Method);
        if (method == null) throw new InvalidOperationException($"Method '{Method}' not found in type '{Type}'.");

        // Deserialize the payload into the method's parameters
        var parameterInfos = method.GetParameters();
        var parameters = new object[parameterInfos.Length];
        var arguments = JsonSerializer.Deserialize<object[]>(Payload);
        if (arguments == null || arguments.Length != parameterInfos.Length)
            throw new InvalidOperationException("Payload does not match method parameters.");

        for (int i = 0; i < parameterInfos.Length; i++)
        {
            var parameterType = parameterInfos[i].ParameterType;
            parameters[i] = JsonSerializer.Deserialize(JsonSerializer.Serialize(arguments[i]), parameterType);
        }
        
        var handlerInstance = Activator.CreateInstance(handlerType);
        if (handlerInstance == null) throw new InvalidOperationException($"Handler instance for type '{Type}' not found.");

        // Invoke the method
        var result = method.Invoke(handlerInstance, parameters);

        // Await the result if it's a Task
        if (result is Task taskResult)
        {
            await taskResult;
        }
    }
    
    private void FingerprintJob()
    {
        if (Fingerprint != null)
            return;

        var js = System.Text.Json.JsonSerializer.Serialize(Payload);
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(Queue + js));
        Fingerprint = BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
    
    public TaskTowerJob MarkCompleted(DateTimeOffset ranAt)
    {
        Status = JobStatus.Completed();
        RanAt = ranAt;
        return this;
    }
    
    public TaskTowerJob MarkAsFailed(string error)
    {
        Status = JobStatus.Failed();
        return this;
    }
    
    public TaskTowerJob BumpRetry()
    {
        if (Retries < MaxRetries)
            Retries++;
        return this;
    }
    
    public TaskTowerJob ChangeRunAfter(DateTimeOffset runAfter)
    {
        RunAfter = runAfter;
        return this;
    }
    
    public TaskTowerJob ChangeDeadline(DateTimeOffset? deadline)
    {
        Deadline = deadline;
        return this;
    }
    
    public TaskTowerJob ChangMaxRetries(int maxRetries)
    {
        MaxRetries = maxRetries;
        return this;
    }

    private TaskTowerJob() { } // EF Core
}