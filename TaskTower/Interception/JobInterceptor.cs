namespace TaskTower.Interception;

public class JobInterceptor
{
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
}