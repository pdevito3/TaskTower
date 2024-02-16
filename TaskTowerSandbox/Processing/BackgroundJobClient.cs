namespace TaskTowerSandbox.Processing;

using System.Linq.Expressions;
using System.Text.Json;
using Configurations;
using Domain.TaskTowerJob;
using Domain.TaskTowerJob.Models;
using Npgsql;
using Dapper;
using Database;
using Microsoft.Extensions.Options;

public interface IBackgroundJobClient
{
    Task<Guid> Enqueue(Expression<Action> methodCall, CancellationToken cancellationToken = default);
    Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default);
    Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default);
    
    // TODO see if i can get this one working
    Task<Guid> Enqueue(Expression<Func<Task>> methodCall, CancellationToken cancellationToken = default);
}

public class BackgroundJobClient : IBackgroundJobClient
{
    private readonly IOptions<TaskTowerOptions> _options;
    private readonly TaskTowerDbContext _dbContext;

    public BackgroundJobClient(IOptions<TaskTowerOptions> options, TaskTowerDbContext dbContext)
    {
        _options = options;
        _dbContext = dbContext;
    }

    public async Task<Guid> Enqueue(Expression<Action> methodCall, CancellationToken cancellationToken = default)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null) throw new InvalidOperationException("Expression body is not a method call.");
        var methodName = GetMethodAndParameterFoundation(methodCallExpression,
            out var parameterTypes);
        var handlerType = ExtractSimpleHandlerType(methodCallExpression, out var handlerTypeName);
        var serializedArguments = SerializedArguments(methodCallExpression!);

        var queueForThisType = GetQueue(handlerType!);
        var jobForCreation = new TaskTowerJobForCreation()
        {
            Queue = queueForThisType,
            Type = handlerTypeName!,
            Method = methodName,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
        };
        var job = TaskTowerJob.Create(jobForCreation);

        await CreateJob(job, cancellationToken);

        return job.Id;
    }

    public async Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null) throw new InvalidOperationException("Expression body is not a method call.");
        var methodName = GetMethodAndParameterFoundation(methodCallExpression,
            out var parameterTypes);
        
        var handlerType = ExtractTypedHandler<T>(out var handlerTypeName);
        var serializedArguments = SerializedArguments(methodCallExpression);

        var queueForThisType = GetQueue(handlerType);
        var jobForCreation = new TaskTowerJobForCreation()
        {
            Queue = queueForThisType,
            Type = handlerTypeName,
            Method = methodName,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
        };
        var job = TaskTowerJob.Create(jobForCreation);
        
        await CreateJob(job, cancellationToken);

        return job.Id;
    }
    
    public async Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall, CancellationToken cancellationToken = default)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null) throw new InvalidOperationException("Expression body is not a method call.");

        var methodName = GetMethodAndParameterFoundation(methodCallExpression, out var parameterTypes);
        var handlerType = ExtractTypedHandler<T>(out var handlerTypeName);
        var serializedArguments = SerializedArguments(methodCallExpression);

        var queueForThisType = GetQueue(handlerType);
        var jobForCreation = new TaskTowerJobForCreation
        {
            Queue = queueForThisType,
            Type = handlerTypeName!,
            Method = methodName,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
        };
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
        // TODO connection string check
        await using var conn = new NpgsqlConnection(_options.Value?.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        
        await conn.ExecuteAsync(
            "INSERT INTO jobs (id, queue, type, method, parameter_types, payload, max_retries, run_after, ran_at, deadline, created_at, fingerprint, status, retries) " +
            "VALUES (@Id, @Queue, @Type, @Method, @ParameterTypes, @Payload::jsonb, @MaxRetries, @RunAfter, @RanAt, @Deadline, @CreatedAt, @Fingerprint, @Status, @Retries)",
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
                job.Fingerprint,
                Status = job.Status.Value,
                job.Retries,
                job.RanAt
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

        // Extract the method parameters types
        parameterTypes = method.GetParameters()
            .Select(p => p.ParameterType.AssemblyQualifiedName)
            .Where(p => p != null)
            .ToArray();
        return methodName;
    }
}