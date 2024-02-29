namespace TaskTower.Processing;

using System.Linq.Expressions;
using System.Text.Json;
using Configurations;
using Dapper;
using Database;
using Domain.TaskTowerJob;
using Domain.TaskTowerJob.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware;
using Npgsql;

public interface IBackgroundJobClient
{
    Task<Guid> Enqueue(Expression<Action> methodCall, CancellationToken cancellationToken = default);
    Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default);
    Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default);
    
    Task<Guid> Enqueue(Expression<Action> methodCall, string? queue, CancellationToken cancellationToken = default);
    Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall, string? queue, CancellationToken cancellationToken = default);
    Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, string? queue, CancellationToken cancellationToken = default);
    
    Task<Guid> Schedule(Expression<Action> methodCall, TimeSpan delay, CancellationToken cancellationToken = default);
    Task<Guid> Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay, CancellationToken cancellationToken = default);
    Task<Guid> Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay, CancellationToken cancellationToken = default);
    
    
    Task<Guid> Schedule(Expression<Action> methodCall, TimeSpan delay, string? queue, CancellationToken cancellationToken = default);
    Task<Guid> Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay, string? queue, CancellationToken cancellationToken = default);
    Task<Guid> Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay, string? queue, CancellationToken cancellationToken = default);
    
    IScheduleBuilder Schedule(Expression<Action> methodCall, CancellationToken cancellationToken = default);
    IScheduleBuilder Schedule<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default);
    IScheduleBuilder Schedule<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default);
    IScheduleBuilder Schedule(Expression<Action> methodCall, string? queue, CancellationToken cancellationToken = default);
    IScheduleBuilder Schedule<T>(Expression<Action<T>> methodCall, string? queue, CancellationToken cancellationToken = default);
    IScheduleBuilder Schedule<T>(Expression<Func<T, Task>> methodCall, string? queue, CancellationToken cancellationToken = default);
    
    // TODO see if i can get this one working
    // Task<Guid> Enqueue(Expression<Func<Task>> methodCall, CancellationToken cancellationToken = default);
    // Task<Guid> Enqueue(Expression<Func<Task>> methodCall, string? queue, CancellationToken cancellationToken = default);
    // Task<Guid> Schedule(Expression<Func<Task>> methodCall, TimeSpan delay, CancellationToken cancellationToken = default);
    
    Task<IBackgroundJobClient> TagJobAsync(Guid jobId, string tag, CancellationToken cancellationToken = default);
    IBackgroundJobClient TagJob(Guid jobId, string tag);
    Task<IBackgroundJobClient> TagJobAsync(Guid jobId, IEnumerable<string> tags, CancellationToken cancellationToken = default);
    IBackgroundJobClient TagJob(Guid jobId, IEnumerable<string> tags);
    IBackgroundJobClient TagJob(Guid jobId, params string[] tags);
    
    IBackgroundJobClient WithContext<TContextualizer>() 
        where TContextualizer : IJobContextualizer;
}

public class BackgroundJobClient : IBackgroundJobClient
{
    private readonly IOptions<TaskTowerOptions> _options;
    private readonly ILogger _logger;
    private List<IJobContextualizer> _jobContextualizers = new List<IJobContextualizer>();

    public BackgroundJobClient(IOptions<TaskTowerOptions> options, ILogger<BackgroundJobClient> logger)
    {
        _options = options;
        _logger = logger;
    }
    
    public IBackgroundJobClient WithContext<TContextualizer>() 
        where TContextualizer : IJobContextualizer
    {
        _jobContextualizers.Add(Activator.CreateInstance<TContextualizer>());
        return this;
    }
    
    public IScheduleBuilder Schedule(Expression<Action> methodCall, CancellationToken cancellationToken = default)
        => new ScheduleBuilder<Action>(this, methodCall, cancellationToken);
    public IScheduleBuilder Schedule<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default)
        => new ScheduleBuilder<T>(this, methodCall, cancellationToken);
    public IScheduleBuilder Schedule<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default)
        => new ScheduleBuilder<T>(this, methodCall, cancellationToken);
    public IScheduleBuilder Schedule(Expression<Action> methodCall, string? queue, CancellationToken cancellationToken = default)
        => new ScheduleBuilder<Action>(this, methodCall, queue, cancellationToken);
    public IScheduleBuilder Schedule<T>(Expression<Action<T>> methodCall, string? queue, CancellationToken cancellationToken = default)
        => new ScheduleBuilder<T>(this, methodCall, queue, cancellationToken);
    public IScheduleBuilder Schedule<T>(Expression<Func<T, Task>> methodCall, string? queue, CancellationToken cancellationToken = default)
        => new ScheduleBuilder<T>(this, methodCall, queue, cancellationToken);
    
    public async Task<Guid> Enqueue(Expression<Action> methodCall, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, null, null, cancellationToken);
    public async Task<Guid> Enqueue(Expression<Action> methodCall, string? queue, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, null, queue, cancellationToken);
    public async Task<Guid> Schedule(Expression<Action> methodCall, TimeSpan delay, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, delay, null, cancellationToken);
    public async Task<Guid> Schedule(Expression<Action> methodCall, TimeSpan delay, string? queue, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, delay, queue, cancellationToken);
    
    private async Task<Guid> ScheduleJob(Expression<Action> methodCall, TimeSpan? delay, string? queue, CancellationToken cancellationToken = default)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null) throw new InvalidOperationException("Expression body is not a method call.");
        var methodName = GetMethodAndParameterFoundation(methodCallExpression, out var parameterTypes);
        var handlerType = ExtractSimpleHandlerType(methodCallExpression, out var handlerTypeName);
        var serializedArguments = SerializedArguments(methodCallExpression!);

        var queueForThisType = GetQueue(handlerType!, queue);
        var jobForCreation = new TaskTowerJobForCreation()
        {
            Queue = queueForThisType,
            Type = handlerTypeName!,
            Method = methodName!,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
            MaxRetries = _options?.Value?.GetMaxRetryCount(handlerType)
        };
        if (delay.HasValue)
        {
            jobForCreation.RunAfter = DateTimeOffset.UtcNow.Add(delay.Value);
        }
        
        var job = TaskTowerJob.Create(jobForCreation);

        await CreateJob(job, cancellationToken);

        return job.Id;
    }

    public async Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, null, null, cancellationToken);
    public async Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, string? queue, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, null, queue, cancellationToken);
    public async Task<Guid> Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, delay, null, cancellationToken);
    public async Task<Guid> Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay, string? queue, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, delay, queue, cancellationToken);
    
    private async Task<Guid> ScheduleJob<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay, string? queue, CancellationToken cancellationToken = default)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null) throw new InvalidOperationException("Expression body is not a method call.");
        var methodName = GetMethodAndParameterFoundation(methodCallExpression, out var parameterTypes);
        
        var handlerType = ExtractTypedHandler<T>(out var handlerTypeName);
        var serializedArguments = SerializedArguments(methodCallExpression);

        var queueForThisType = GetQueue(handlerType, queue);
        var jobForCreation = new TaskTowerJobForCreation()
        {
            Queue = queueForThisType,
            Type = handlerTypeName!,
            Method = methodName!,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
            MaxRetries = _options?.Value?.GetMaxRetryCount(handlerType)
        };
        if (delay.HasValue)
        {
            jobForCreation.RunAfter = DateTimeOffset.UtcNow.Add(delay.Value);
        }
        
        var job = TaskTowerJob.Create(jobForCreation);
        
        await CreateJob(job, cancellationToken);

        return job.Id;
    }
    
    public async Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, null, null, cancellationToken);
    public async Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall, string? queue, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, null, queue, cancellationToken);
    public async Task<Guid> Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, delay, null, cancellationToken);
    public async Task<Guid> Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay, string? queue, CancellationToken cancellationToken = default)
        => await ScheduleJob(methodCall, delay, queue, cancellationToken);
    
    private async Task<Guid> ScheduleJob<T>(Expression<Action<T>> methodCall, TimeSpan? delay, string? queue, CancellationToken cancellationToken = default)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null) throw new InvalidOperationException("Expression body is not a method call.");

        var methodName = GetMethodAndParameterFoundation(methodCallExpression, out var parameterTypes);
        var handlerType = ExtractTypedHandler<T>(out var handlerTypeName);
        var serializedArguments = SerializedArguments(methodCallExpression);

        var queueForThisType = GetQueue(handlerType, queue);
        var jobForCreation = new TaskTowerJobForCreation
        {
            Queue = queueForThisType,
            Type = handlerTypeName!,
            Method = methodName!,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
            MaxRetries = _options?.Value?.GetMaxRetryCount(handlerType)
        };
        
        if (delay.HasValue)
        {
            jobForCreation.RunAfter = DateTimeOffset.UtcNow.Add(delay.Value);
        }
        
        var job = TaskTowerJob.Create(jobForCreation);

        await CreateJob(job, cancellationToken);

        return job.Id;
    }

    public async Task<Guid> Enqueue(Expression<Func<Task>> methodCall, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        
        // TODO i think this part is done properly, but the invoke on the job is unhappy
        // var methodCallExpression = methodCall.Body as MethodCallExpression;
        // if (methodCallExpression == null) throw new InvalidOperationException("Expression body is not a method call.");
        //
        // var methodName = GetMethodAndParameterFoundation(methodCallExpression, out var parameterTypes);
        // var handlerType = ExtractSimpleHandlerType(methodCallExpression, out var handlerTypeName);
        // var serializedArguments = SerializedArguments(methodCallExpression);
        //
        // var queueForThisType = GetQueue(handlerType!);
        // var jobForCreation = new TaskTowerJobForCreation
        // {
        //     Queue = queueForThisType,
        //     Type = handlerTypeName!,
        //     Method = methodName,
        //     ParameterTypes = parameterTypes ?? Array.Empty<string>(),
        //     Payload = serializedArguments,
        // };
        // var job = TaskTowerJob.Create(jobForCreation);
        //
        // await CreateJob(job, cancellationToken);
        //
        // return job.Id;
    }
    
    public async Task<IBackgroundJobClient> TagJobAsync(Guid jobId, string tag, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_options.Value?.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        InsertTag(jobId, conn, tag, _logger);
        
        return this;
    }
    
    public IBackgroundJobClient TagJob(Guid jobId, IEnumerable<string> tags)
    {
        using var conn = new NpgsqlConnection(_options.Value?.ConnectionString);
        conn.Open();
        
        foreach (var tag in tags)
        {
            InsertTag(jobId, conn, tag, _logger);
        }
        
        return this;
    }

    public IBackgroundJobClient TagJob(Guid jobId, string tag)
    {
        using var conn = new NpgsqlConnection(_options.Value?.ConnectionString);
        conn.Open();
        InsertTag(jobId, conn, tag, _logger);
        
        return this;
    }
    
    public IBackgroundJobClient TagJob(Guid jobId, params string[] tags)
        => TagJob(jobId, tags.AsEnumerable()); 
    
    public async Task<IBackgroundJobClient> TagJobAsync(Guid jobId, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_options.Value?.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        
        foreach (var tag in tags)
        {
            InsertTag(jobId, conn, tag, _logger);
        }
        
        return this;
    }

    private static void InsertTag(Guid jobId, NpgsqlConnection conn, string tag, ILogger logger)
    {
        try
        {
            conn.Execute(
                $"INSERT INTO {MigrationConfig.SchemaName}.tags (job_id, name) VALUES (@JobId, @Name)",
                new
                {
                    JobId = jobId,
                    Name = tag
                });
        }
        catch (PostgresException e) when (e.SqlState == "23505")
        {
            logger.LogInformation("Tag '{Tag}' already exists for job '{JobId}'", tag, jobId);
        }
    }

    private string? GetQueue(Type handlerType, string? givenQueue = null)
    {
        if (!string.IsNullOrWhiteSpace(givenQueue))
            return givenQueue;
        
        var queueAssignments = _options.Value?.QueueAssignments;
        var queueForThisType = queueAssignments?.FirstOrDefault(qa => qa.Key == handlerType).Value;
        return queueForThisType;
    }

    private async Task CreateJob(TaskTowerJob job, CancellationToken cancellationToken = default)
    {
        var creationContext = new JobContext(job);
        foreach (var jobContextualizer in _jobContextualizers)
        {
            jobContextualizer?.EnrichContext(creationContext);
        }
        
        // TODO connection string check
        await using var conn = new NpgsqlConnection(_options.Value?.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        
        await conn.ExecuteAsync(
            $"INSERT INTO {MigrationConfig.SchemaName}.jobs (id, queue, type, method, parameter_types, payload, max_retries, run_after, ran_at, deadline, created_at, status, retries, context_parameters) " +
            "VALUES (@Id, @Queue, @Type, @Method, @ParameterTypes, @Payload::jsonb, @MaxRetries, @RunAfter, @RanAt, @Deadline, @CreatedAt, @Status, @Retries, @ContextParameters::jsonb)",
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
                ContextParameters = job.RawContextParameters
            });
        
        // _dbContext.Jobs.Add(job);
        // await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string SerializedArguments(MethodCallExpression methodCallExpression)
    {
        var serializableArguments = methodCallExpression.Arguments
            .Select(arg =>
            {
                // Check if the argument is a lambda expression or a delegate (which cannot be serialized directly)
                if (arg.NodeType == ExpressionType.Lambda || arg.Type.BaseType == typeof(MulticastDelegate))
                {
                    // Handle non-serializable types differently, e.g., by replacing with a placeholder or by excluding from serialization
                    return null; // Or use a specific placeholder value that indicates a non-serializable argument
                }
    
                // Convert the argument to object and compile the expression to invoke it
                var objectMember = Expression.Convert(arg, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                return getter();
            })
            .Where(arg => arg != null) // Exclude non-serializable or placeholder values if you choose to use them
            .ToArray();
    
        var serializedArguments = JsonSerializer.Serialize(serializableArguments);
        return serializedArguments;
    }

    private static Type? ExtractSimpleHandlerType(MethodCallExpression? methodCallExpression, out string? handlerTypeName)
    {
        var handlerType = methodCallExpression!.Method.DeclaringType;
        if (handlerType == null)
            throw new InvalidOperationException("Method declaring type is null.");
        handlerTypeName = handlerType.AssemblyQualifiedName;
        if (handlerTypeName == null)
            throw new InvalidOperationException($"Handler type '{handlerType.FullName}' not found.");
        return handlerType;
    }

    private static Type ExtractTypedHandler<T>(out string? handlerTypeName)
    {
        var handlerType = typeof(T);
        handlerTypeName = handlerType.AssemblyQualifiedName;
        if (handlerTypeName == null)
            throw new InvalidOperationException($"Handler type '{typeof(T).FullName}' not found.");
        return handlerType;
    }

    private static string? GetMethodAndParameterFoundation(MethodCallExpression methodCallExpression,
        out string[] parameterTypes)
    {
        // Extract the method info
        var method = methodCallExpression.Method;
        var methodName = method.Name;
        
        if(methodName == null)
            throw new InvalidOperationException("Method name is null.");

        // Extract the method parameters types
        parameterTypes = method.GetParameters()
            .Select(p => p.ParameterType.AssemblyQualifiedName)
            .Where(p => p != null)
            .ToArray();
        
        return methodName;
    }
}