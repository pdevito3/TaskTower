namespace TaskTower.Interception;

using Domain.InterceptionStages;

public class JobInterceptor
{
    internal InterceptionStage InterceptionStage { get; private set; }
    
    private readonly IServiceProvider _serviceProvider;

    public JobInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public virtual JobServiceProvider Intercept(JobInterceptorContext interceptorContext)
    {
        return new JobServiceProvider(_serviceProvider);
    }

    public virtual JobServiceProvider Intercept()
    {
        return new JobServiceProvider(_serviceProvider);
    }
    
    internal void SetInterceptionStage(InterceptionStage interceptionStage)
    {
        InterceptionStage = interceptionStage;
    }
}