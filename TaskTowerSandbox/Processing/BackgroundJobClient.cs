namespace TaskTowerSandbox.Processing;

using System.Linq.Expressions;
using System.Text.Json;
using Database;
using Domain.TaskTowerJob;
using Domain.TaskTowerJob.Models;
using Serilog;

public class BackgroundJobClient
{
    public async Task<Guid> Enqueue<T>(Expression<Func<T, Task>> methodCall, TaskTowerDbContext context)
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

        var serializedArguments = System.Text.Json.JsonSerializer.Serialize(arguments);

        var jobForCreation = new TaskTowerJobForCreation()
        {
            Queue = Guid.NewGuid().ToString(),
            Type = handlerTypeName,
            Method = methodName,
            ParameterTypes = parameterTypes ?? Array.Empty<string>(),
            Payload = serializedArguments,
        };
        var job = TaskTowerJob.Create(jobForCreation);
        
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        return job.Id;
    }
}

public class DoAThing
{
    // command
    public sealed record Command(string Data);
        
    public async Task Handle(Command request)
    {
        await Task.Delay(1000);
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}