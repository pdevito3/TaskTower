namespace TaskTower.Processing;

using Domain;
using Domain.TaskTowerJob;
using System.Text.Json;

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
    private readonly List<ContextParameter> _contextParameters = new();
    public IReadOnlyList<ContextParameter> ContextParameters => _contextParameters;
    
    public T? GetContextParameter<T>(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof (name));
        
        var contextParameter = _contextParameters.FirstOrDefault(x => x.Name == name);
        
        if (contextParameter == null)
            throw new ArgumentException($"Context parameter with name {name} not found");
        
        var contextParameterType = Type.GetType(contextParameter.Type);
        
        if (contextParameterType != typeof(T))
        {
            // TODO log?
            return default;
        }
        
        if (contextParameterType == null)
            throw new ArgumentException($"Context parameter with name {name} has a null type");
        
        try
        {
            // Assuming contextParameter.Value is either a string containing JSON or a JsonElement
            var jsonValue = contextParameter.Value is JsonElement jsonElement
                ? jsonElement.GetRawText()
                : contextParameter?.Value?.ToString();
            //
            // // if (Nullable.GetUnderlyingType(contextParameterType) != null)
            // //     return null; // It's nullable, return null
            //
            // if (jsonValue == null)
            //     jsonValue = "{}";
            
            if (jsonValue == null)
                // TODO need to add null handling
                throw new ArgumentException($"Context parameter with name {name} has a null value");
            
            return JsonSerializer.Deserialize<T>(jsonValue);
        }
        catch (JsonException)
        {
            throw new ArgumentException($"Context parameter with name {name} could not be deserialized to type {typeof(T).Name}");
        }
    }
    
    public static JobContext Create(TaskTowerJob job)
    {
        var context = new JobContext();
        context._contextParameters.AddRange(job.ContextParameters);
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
