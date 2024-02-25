namespace TaskTowerSandbox.Sandboxing;

using TaskTower.Processing;

/// <summary>
/// Adds user context when enqueuing your job that can be used by the job via the activator
/// </summary>
// public class CurrentUserJobMiddlewareWasAttribute : IJobCreationMiddleware
// {
//     public void OnCreating(CreatingContext context)
//     {
//         var argue = context.Job.ContextParameters.FirstOrDefault(x => x is IJobWithUserContext);
//         if (argue == null)
//             throw new Exception($"This job does not implement the {nameof(IJobWithUserContext)} interface");
//
//         var jobParameters = argue as IJobWithUserContext;
//         var user = jobParameters?.User;
//
//         if(user == null)
//             throw new Exception($"A User could not be established");
//
//         context.SetJobContextParameter("User", user);
//     }
// }

public class JobUserMiddlewareWasAttribute : IJobCreationMiddleware
{
    public void OnCreating(CreatingContext context)
    {
        var user = "job-user-346f9812-16da-4a72-9db2-f066661d6593";
        context.SetJobContextParameter("User", user);
    }
}

public class JobWithUserContextInterceptor : JobInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    
    public JobWithUserContextInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override JobServiceProvider Intercept(JobContext context)
    {
        var user = context.GetContextParameter<object>("User");
        
        if (user == null)
        {
            return base.Intercept(context);
        }

        var userContextForJob = _serviceProvider.GetRequiredService<IJobContextAccessor>();
        userContextForJob.UserContext = new JobWithUserContext {User = user.ToString()};

        return new JobServiceProvider(_serviceProvider);
    }
}

public interface IJobWithUserContext
{
    public string? User { get; init; }
}
public class JobWithUserContext : IJobWithUserContext
{
    public string? User { get; init; }
}
public interface IJobContextAccessor
{
    JobWithUserContext? UserContext { get; set; }
}
public class JobContextAccessor : IJobContextAccessor
{
    public JobWithUserContext? UserContext { get; set; }
}