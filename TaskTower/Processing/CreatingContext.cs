namespace TaskTower.Processing;

using Domain.TaskTowerJob;
using Microsoft.Extensions.DependencyInjection;

public class CreatingContext
{
    public TaskTowerJob Job { get; private set; }
    public void SetJobContextParameter(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof (name));
        Job.SetContextParameter(name, value);
    }
    
    internal CreatingContext(TaskTowerJob job)
    {
        Job = job;
    }
}

/// <summary>
/// Allows you to add context when enqueuing your job during creation that can be used by the job activator
/// </summary>
public interface IJobCreationMiddleware
{
    public void OnCreating(CreatingContext context);
}

public class JobContext
{
    private readonly Dictionary<string, object> _contextParameters = new();
    public IReadOnlyDictionary<string, object> ContextParameters => _contextParameters;
    
    public void SetContextParameter(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof (name));
        _contextParameters[name] = value;
    }
    
    public T? GetContextParameter<T>(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof (name));
        return _contextParameters.TryGetValue(name, out var parameter) ? (T) parameter : default (T);
    }
    
    public static JobContext Create(TaskTowerJob job)
    {
        var context = new JobContext();
        foreach (var contextParameter in job.ContextParameters)
        {
            context.SetContextParameter(contextParameter.Key, contextParameter.Value);
        }
        return context;
    }
}

public class JobInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    public JobInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public virtual JobServiceProvider Intercept(JobContext context)
    {
        return new JobServiceProvider(_serviceProvider);
    }

    public virtual JobServiceProvider Intercept()
    {
        return new JobServiceProvider(_serviceProvider);
    }
}

public class JobServiceProvider
{
    private readonly IServiceProvider _serviceProvider;

    public JobServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    public IServiceProvider GetServiceProvider()
    {
        return _serviceProvider;
    }
}


// ---- this will go in the client app
