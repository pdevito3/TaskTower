namespace TaskTower.Interception;

using Microsoft.Extensions.DependencyInjection;

internal class TaskTowerRunnerContextInterceptor : JobInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    
    public TaskTowerRunnerContextInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override JobServiceProvider Intercept(JobInterceptorContext interceptorContext)
    {
        var taskTowerRunnerContext = _serviceProvider.GetRequiredService<ITaskTowerRunnerContext>();
        taskTowerRunnerContext.JobId = interceptorContext.Job.Id;

        return new JobServiceProvider(_serviceProvider);
    }
}