namespace RecipeManagement.Extensions;

using System.Linq.Expressions;
using Databases;
using Domain;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecipeManagement.Services;
using Serilog;

public interface IJobExecutor : IRecipeManagementScopedService
{
    Task ExecuteJob(Guid jobId, CancellationToken cancellationToken);
    Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall);
}

public class JobExecutor : IJobExecutor
{
    private readonly RecipesDbContext _context;

    public JobExecutor(RecipesDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Enqueue<T>(Expression<Action<T>> methodCall)
    {
        var methodCallExpression = methodCall.Body as MethodCallExpression;
        if (methodCallExpression == null)
        {
            throw new ArgumentException("The provided expression does not represent a method call.");
        }

        var method = methodCallExpression.Method;

        var arguments = methodCallExpression.Arguments
            .Select(arg => Expression.Lambda(arg).Compile().DynamicInvoke())
            .ToList(); 

        await using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var jobEntity = new Job
            {
                TypeName = typeof(T).AssemblyQualifiedName,
                MethodName = method.Name,
                Parameters = arguments != null ? JsonSerializer.Serialize(arguments) : null,
                State = JobState.Enqueued,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Jobs.Add(jobEntity);
            await _context.SaveChangesAsync();
            
            var notifyCommand = _context.Database.GetDbConnection().CreateCommand();
            notifyCommand.CommandText = "NOTIFY new_job;";
            await notifyCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return jobEntity.Id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteJob(Guid jobId, CancellationToken cancellationToken)
    {
        var jobEntity = await _context.Jobs.FindAsync(jobId);
        if (jobEntity == null)
        {
            throw new InvalidOperationException("Job not found.");
        }

        jobEntity.State = JobState.Processing;
        jobEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var type = Type.GetType(jobEntity.TypeName);
            if (type == null)
            {
                throw new InvalidOperationException("Type not found.");
            }

            var method = type.GetMethod(jobEntity.MethodName);
            if (method == null)
            {
                throw new InvalidOperationException("Method not found.");
            }

            var parameterInfos = method.GetParameters();
            if (parameterInfos.Length > 0 && !string.IsNullOrEmpty(jobEntity.Parameters))
            {
                using var doc = JsonDocument.Parse(jobEntity.Parameters);
                var parametersJArray = doc.RootElement.EnumerateArray().ToArray();
                var parameters = new object[parameterInfos.Length];
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    parameters[i] = JsonSerializer.Deserialize(
                        parametersJArray[i].GetRawText(), 
                        parameterInfos[i].ParameterType);
                }

                var instance = Activator.CreateInstance(type);
                method.Invoke(instance, parameters);
                jobEntity.State = JobState.Succeeded;
            }
            else
            {
                var instance = Activator.CreateInstance(type);
                method.Invoke(instance, null);
                jobEntity.State = JobState.Succeeded;
            }
        }
        catch (Exception ex)
        {
            jobEntity.ErrorMessage = $"{ex.Message}\n{ex.StackTrace}";
            jobEntity.State = JobState.Failed;
            Log.Error($"There was an error executing job {jobId}: {ex.Message}");
        }
        finally
        {
            _context.Jobs.Update(jobEntity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
