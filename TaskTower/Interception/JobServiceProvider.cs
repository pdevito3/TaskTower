namespace TaskTower.Interception;

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