namespace TaskTower.Interception;

using System.Text.Json;
using TaskTower.Domain;
using TaskTower.Domain.TaskTowerJob;

/// <summary>
/// Provides context from a job during interceptor execution
/// </summary>
public class JobInterceptorContext
{
    private readonly List<ContextParameter> _contextParameters = new();
    public IReadOnlyList<ContextParameter> ContextParameters => _contextParameters;
    public TaskTowerJob Job { get; private set; }
    public ErrorDetails? ErrorDetails { get; private set; }
    
    
    public T? GetContextParameter<T>(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof (name));
        
        var contextParameter = _contextParameters.FirstOrDefault(x => x.Name == name);
        
        if (contextParameter == null)
            throw new ArgumentException($"Context parameter with name {name} not found");
        
        var contextParameterType = Type.GetType(contextParameter.Type);
        
        if (contextParameterType != typeof(T))
            throw new ArgumentException($"Context parameter with name {name} does not match stored type {typeof(T).Name}");
        
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
    
    internal static JobInterceptorContext Create(TaskTowerJob job)
    {
        var context = new JobInterceptorContext();
        context._contextParameters.AddRange(job.ContextParameters);
        context.Job = job;
        return context;
    }
    
    internal void SetErrorDetails(ErrorDetails errorDetails)
    {
        ErrorDetails = errorDetails;
    }
}