namespace TaskTowerSandbox.Sandboxing;

using TaskTower.Interception;
using TaskTower.Middleware;
using TaskTower.Processing;

public class SlackSaysDeathInterceptor : JobInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    
    public SlackSaysDeathInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override JobServiceProvider Intercept(JobInterceptorContext interceptorContext)
    {
        var jobId = interceptorContext.Job.Id;
        var errorDetails = interceptorContext.ErrorDetails;
        var fakeSlackService = _serviceProvider.GetRequiredService<FakeSlackService>();
        
        fakeSlackService.SendMessage("death", $"""
                                               Job {jobId} has died with error: {errorDetails?.Message} at {errorDetails?.OccurredAt}. Here's the details
                                               
                                               {errorDetails?.Details}
                                               """);

        return new JobServiceProvider(_serviceProvider);
    }
}
