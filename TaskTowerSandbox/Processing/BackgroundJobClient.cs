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
    Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default);
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

    public async Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken = default)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null)
            throw new InvalidOperationException("Expression body is not a method call.");

        // Extract the handler type
        var handlerType = typeof(T);
        var handlerTypeName = handlerType.AssemblyQualifiedName;
        if (handlerTypeName == null)
            throw new InvalidOperationException($"Handler type '{typeof(T).FullName}' not found.");

        // Extract the method info
        var method = methodCallExpression.Method;
        var methodName = method.Name;

        // Extract the method parameters types
        string[] parameterTypes = method.GetParameters()
            .Select(p => p.ParameterType.AssemblyQualifiedName)
            .Where(p => p != null)
            .ToArray();

        // Serialize the arguments from the method call expression
        var arguments = methodCallExpression.Arguments.Select(arg =>
        {
            // You might need to adjust the logic for constant/other expressions
            var objectMember = Expression.Convert(arg, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }).ToArray();

        var serializedArguments = JsonSerializer.Serialize(arguments);

        var jobForCreation = new TaskTowerJobForCreation()
        {
            Queue = Guid.NewGuid().ToString(),
            Type = handlerTypeName,
            Method = methodName,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
        };
        var job = TaskTowerJob.Create(jobForCreation);
        
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

        return job.Id;
    }
}