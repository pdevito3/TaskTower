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
}

public class JobActivator
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public JobActivator(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory != null 
            ? serviceScopeFactory 
            : throw new ArgumentNullException(nameof (serviceScopeFactory));
    }

    public virtual JobServiceScope BeginScope(JobContext context)
    {
        return new JobServiceScope(_serviceScopeFactory.CreateScope());
    }

    public virtual JobServiceScope BeginScope()
    {
        return new JobServiceScope(_serviceScopeFactory.CreateScope());
    }
}

public class JobServiceScope
{
    private readonly IServiceScope _serviceScope;

    public JobServiceScope(IServiceScope serviceScope)
    {
        _serviceScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
    }
    
    public T? GetService<T>()
    {
        return _serviceScope.ServiceProvider.GetService<T>();
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}


// ---- this will go in the client app
