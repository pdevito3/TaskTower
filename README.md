# Task Tower

[//]: # (# IMAGE)

## Simple, reliable & efficient background jobs in .NET - an alternative to HangFire

Task Tower is a .NET library that provides a simple, reliable and efficient way to run background jobs in your .NET 
applications. It is an alternative to Hangfire that is built on top of PostgreSQL's `LISTEN`/`NOTIFY` channels to 
reduce polling frequency and enable immediate message delivery.

> [!IMPORTANT]  
> Task Tower is currently in development and is subject to change. Current major version is zero (v0.x.x) to 
> accommodate rapid development and fast iteration while getting early feedback from users (feedback on APIs and bugs 
> are appreciated!). The public API could change without a major version update before v1.0.0 release.


## Features

- Enqueued Jobs
- Scheduled Jobs
- Retries of failed jobs with incremental backoff
- Various queue prioritizations
- Job tags for categorization and filtering
- Job Contextualizers and Interceptors for lifecycle actions
- Uses PostgreSQL's `LISTEN`/`NOTIFY` channels reduce polling frequency enabling immediate message delivery
- Rich, modern Web UI for easy and in depth visibility (WIP)

## Coming Soon 🚧
- Recurring Jobs
- Batch Jobs
- More UI work
- OTel integration
- Heartbeat monitoring
- Redis transport support
- More interceptors
- Ensure stability

## Quick Start
1. `dotnet new web -n MyTaskTowerApp`
2. `cd MyTaskTowerApp`
3. Add the Task Tower nuget package `dotnet add package TaskTower`
4. Add TestContainers for easy Postgres usage `dotnet add Testcontainers.PostgreSql` (or just use a local db)
5. Update your `Program.cs` to use Task Tower

```csharp
using DotNet.Testcontainers.Builders;
using TaskTower.Configurations;
using TaskTower.Processing;
using Testcontainers.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// spins up a Postgres container for Task Tower (you can also skip this and use a local db)
var dbContainer = new PostgreSqlBuilder()
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
    .Build();
await dbContainer.StartAsync();

// adds the Task Tower UI
builder.Services.AddTaskTower(builder.Configuration, x =>
{
    x.ConnectionString = $"{dbContainer.GetConnectionString()};Pooling=true;MinPoolSize=1;MaxPoolSize=10000;";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// adds the Task Tower UI
app.UseTaskTowerUi();
app.UseHttpsRedirection();

app.MapPost("/console-log", async (HttpContext _, IBackgroundJobClient client) =>
{
    var jobId = await client.Enqueue(() => Console.WriteLine("Hello, from Task Tower!"));
    return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
});

try
{
    await app.RunAsync();
}
finally
{
    await dbContainer.StopAsync();
}
```
5. Run your app `dotnet run`
6. Navigate to `/swagger` and hit the `/console-log` endpoint to enqueue a job
7. Check the console to see the response and go to `/task-tower` to see the Task Tower UI

## Setup
1. Install the Task Tower NuGet package `dotnet add package TaskTower`
2. Add Task Tower to your services. This will setup the Task Tower worker, run database migrations, and configure all the Task Tower services. 
```csharp
builder.Services.AddTaskTower(builder.Configuration, x =>
{
    x.ConnectionString = $"{connection};Pooling=true;MinPoolSize=1;MaxPoolSize=10000;";
});
```
3. Add the task tower dashboard (optional)
```csharp
app.UseTaskTowerUi();
```

## Options
```csharp
builder.Services.AddTaskTower(builder.Configuration, x =>
{
    x.ConnectionString = $"{connection};Pooling=true;MinPoolSize=1;MaxPoolSize=10000;"; // Set the connection string for the Task Tower database
    x.SchemaName = "custom; // Set the schema name for the Task Tower database (defaults to `task_tower`)
    x.BackendConcurrency = 3; // The total number of backend processes available to process jobs (defaults to `Environment.ProcessorCount`)
    x.QueuePrioritization = QueuePrioritization.None(); // Set the queue prioritization strategy
    x.QueuePriorities = new Dictionary<string, int>(); // Establishes the different queues you will use and their priorities
    x.QueueAnnouncementInterval = TimeSpan.FromSeconds(1); // Set the interval at which Task Tower will announce jobs for processing
    x.JobCheckInterval = TimeSpan.FromSeconds(1); // Set the interval at which Task Tower will check for jobs to announce
    x.JobConfigurations = ... // Configures jobs with specific settings
});
```

For Example:
```csharp
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.ConnectionString = "Host=localhost;Port=41444;Database=dev_hello_task_tower_sandbox;Username=postgres;Password=postgres;Pooling=true;MinPoolSize=1;MaxPoolSize=10000;";
    x.BackendConcurrency = 5;
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"critical", 3},
        {"default", 2},
        {"low", 1}
    };
    x.QueuePrioritization = QueuePrioritization.Strict();
    x.AddJobConfiguration<DoAPossiblyFailingThing>()
        .SetQueue("critical")
        .SetDisplayName("Possibly Failing Task")
        .SetMaxRetryCount(2)
        .WithDeathInterceptor<SlackSaysDeathInterceptor>()
        .WithDeathInterceptor<TeamsSaysDeathInterceptor>();
});
```

> [!NOTE]  
> Check intervals have a hard coded minimum of that can not me less than 500ms

## How Task Tower Works

1. Jobs are created (e.g. using an `Enqueue`, `Schedule`, etc.)
    1. When a job is inserted and should be processed immediately, it is immediately marked as `Enqueued` using a database trigger
    2. When a job is inserted and should be processed at a later time, Task Tower will poll at at a `JobCheckInterval` to enqueue jobs at that pace
2. Task Tower will check the queue at an interval of `QueueAnnouncementInterval` based on whatever prioritization strategy has been configured and announce jobs for processing using Postges' `pg_notify`
3. Task Tower will be listening for announced jobs and pick them up for processing
4. Once a job is picked up it will be marked a `Processing` until finished running
    1. Jobs that are stopped mid-processing will be re-queued when starting up
5. Once finished a job will be marked as:
    1. `Completed` if successful
    2. `Failed` if failed with retries remaining
    3. `Dead` if failed with no retries remaining

## Task Tower UI
Task Tower comes with a rich, modern Web UI that provides easy and in depth visibility into your background jobs. 
It's currently still early, but will be expanded leading up to a stable v1 release.

A jobs worklist:
![jobs-worklist.png](DocAssets%2Fjobs-worklist.png)

View job details:
![success-job-view.png](DocAssets%2Fsuccess-job-view.png)

![dead-job-view.png](DocAssets%2Fdead-job-view.png)

## Enqueuing Jobs

Jobs can be enqueued using the `IBackgroundJobClient` interface using several different methods.

### Enqueued Jobs
```csharp
app.MapPost("/console-log", async (HttpContext _, IBackgroundJobClient client) =>
{
    var jobId = await client.Enqueue(() => Console.WriteLine("Hello, from Task Tower!"));
    return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
});
```

You can also enqueue jobs with injection and parameters like this:
```csharp
app.MapPost("/create-injectable-job", async (JobData request, HttpContext http, IBackgroundJobClient client) =>
{
    var command = new DoAnInjectableThing.Command(request.Payload);
    var jobId = await client.Enqueue<DoAnInjectableThing>(x => x.Handle(command));
    return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
});

public class DoAnInjectableThing(IDummyLogger logger, PokeApiService pokeApiService)
{
    public sealed record Command(string Data);
    
    public async Task Handle(Command request)
    {        
        var pokemon = await pokeApiService.GetRandomPokemonAsync();
        Log.Information("I just read a Pokemon with an ID of {PokemonId} and content of {Name}", pokemon.Item1, pokemon.Item2);
        
        logger.Log($"Handled DoAnInjectableThing with data: {request.Data}");
    }
}
```

### Scheduled Jobs
You can also schedule jobs to run at a later time. For example:
```csharp
app.MapPost("/thirty-second-delay", async (HttpContext http, IBackgroundJobClient client) =>
{
    var jobId = await client.Schedule<DoAThing>(x => 
            x.Handle(new DoAThing.Command("this is a scheduled job")), 
        TimeSpan.FromSeconds(30));

    return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
});

public class DoAThing
{
    public sealed record Command(string Data);        
    public async Task Handle(Command request)
    {
        Log.Information("Handled DoAThing with data: {Data}", request.Data);
    }
}
```

Or you can use a fluent API like this:
```csharp
app.MapPost("/fluent-two-second-delay", async (HttpContext http, IBackgroundJobClient client) =>
{
    var jobId = await client.Schedule<DoAThing>(x => 
            x.Handle(new DoAThing.Command("this is a scheduled job")))
        .ToQueue("critical")
        .InSeconds(2);

    return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
});
```

### Recurring Jobs
Coming soon...

### Batch Jobs
Coming soon...

## Queues

There are a various different queuing prioritization strategies that can be used in Task Tower. Jobs with no recognized queue will be considered a lowest priority based on whatever rules apply to the type of prioritization.

> [!NOTE]  
> By default, all jobs will be added to a `default` queue unless otherwise designated.

### None (FIFO)

This prioritization strategy will process jobs using no particular prioritization and just behavior in a simple first-in-first-out (FIFO) fashion.

### Alphanumeric

This prioritization strategy will process jobs using an alphanumeric order based on teh queue name. If you are coming from Hangfire and need pairity with their queue behavior, you can lean on this.

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePrioritization = QueuePrioritization.AlphaNumeric();
    // ....
});
```

### Index

If you need to create multiple queues and want to prioritize processing of all jobs in one queue over other queues, you can use the `Index` option. This means that jobs will be pulled from the queue sorted in the priory designated in the configuration.

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"critical", 3},
        {"default", 2},
        {"low", 1}
    };
    x.QueuePrioritization = QueuePrioritization.Index();
    // ....
});
```

This example will configure Task Tower to have three queue priorities: **critical**, **default**, and **low** with strict priority. When using the `Index` priority, the queues with higher a priority will be prioritized at the top of the queue for processing.

### Strict

If you need to create multiple queues and need to process all tasks in one queue over other queues, you can use the `Strict` option. Only queues that are given a priority will be recognized and ran when using this strategy. Jobs in unprioritized queues (that are not configured with a priority in the Task Tower config) will not be picked up for processing.

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"critical", 3},
        {"default", 2},
        {"low", 1}
    };
    x.QueuePrioritization = QueuePrioritization.Strict();
    // ....
});
```

This example will configure Task Tower to have three queue priorities: **critical**, **default**, and **low** with strict priority. In strict priority mode, the queues with higher priority is always processed first, and queues with lower priority is processed only if all the other queues with higher priorities are empty.

So in this example, tasks in **critical** queue is always processed first. If **critical** queue is empty, then **default** queue is processed. If both **critical** and **default** queue are empty, then **low** queue is processed.

### Weighted 🚧
Coming soon...

The number associated with the queue name is the priority level for the queue.

With this above configuration:

- tasks in **critical** queue will be processed **60%** of the time
- tasks in **default** queue will be processed **30%** of the time
- tasks in **low** queue will be processed **10%** of the time

Example:

```c#
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"high", 60},
        {"default", 30},
        {"low", 10}
    };
    x.QueuePrioritization = QueuePrioritization.Weighted();
    // ....
});
```

## Tags

You can use the background client to add tags to jobs to allow for easier categorization and filtering of jobs, facilitating  quick identification and management of related tasks. Tags also enhance monitoring and debugging capabilities to let you efficiently track job execution and diagnose issues within specific job  groups.

You can add tags to a job using a variety of APIs.

```csharp
var command = new DoAThing.Command("this is a tagged job");
var jobId = await client.Enqueue<DoAThing>(x => x.Handle(command));

client.TagJob(jobId, "tag1")
    .TagJob(jobId, "tag2");

await client.TagJobAsync(jobId, "tag3");
await client.TagJobAsync(jobId,  ["tag4"]);

client.TagJob(jobId, "tag5", "tag6");
client.TagJob(jobId, ["tag7", "tag8", "tag9"]);
client.TagJob(jobId,  ["tag10", "tag11"]);
```

## Job Configuration

There several different options that can be used to configure jobs in Task Tower.

- `SetQueue()` can be used to configure jobs to be sent to a particular queue
    - If a queue is not set, the job will be sent to the `default` queue
- `SetDisplayName()` can be used to give a job a human readable name in the Task Tower UI
- `SetMaxRetryCount()` can be used to set the maximum number of times a job can be retried
    - By default, a job will be retried 10 times with a progressive backoff strategy
- `WithPreProcessingInterceptor()` can be used to add an interceptor to a job that will run before the job is processed
- `WithDeathInterceptor()` can be used to add an interceptor to a job that will run after the job has been marked as `Dead` (i.e. has failed all retries)

For example:
```csharp
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.AddJobConfiguration<DoAPossiblyFailingThing>()
        .SetQueue("critical")
        .SetDisplayName("Possibly Failing Task")
        .SetMaxRetryCount(2)
        .WithPreProcessingInterceptor<JobWithUserContextInterceptor>()
        .WithDeathInterceptor<SlackSaysDeathInterceptor>();
  	//...
});
```

## Job Contextualizers and Interceptors

Task tower providers interceptors for performing activities during various stages of a job's lifecycle.

- `PreProcessing`: runs before processing a job
- `Death`: runs after a job has been marked as `Dead` (i.e. has failed all retries)

For example, if I wanted to send a slack notification when a job is dead, I could make an interceptor like this:

```csharp
public class FakeSlackService()
{
    public void SendMessage(string channel, string message)
    {
        Log.Information("Sending message to the '{Channel}' channel: '{Message}'", channel, message);
    }
}

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
```

And then add it to a job like this in your configuration:

```csharp
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.AddJobConfiguration<DoAPossiblyFailingThing>()
        .SetQueue("critical")
        .SetDisplayName("Possibly Failing Task")
        .SetMaxRetryCount(2)
        .WithDeathInterceptor<SlackSaysDeathInterceptor>();
  	//...
});
```

Interceptors can be stacked if you want:

```csharp
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.AddJobConfiguration<DoAPossiblyFailingThing>()
        .SetQueue("critical")
        .SetDisplayName("Possibly Failing Task")
        .SetMaxRetryCount(2)
        .WithDeathInterceptor<SlackSaysDeathInterceptor>()
        .WithDeathInterceptor<TeamsSaysDeathInterceptor>();
  	//...
});
```

An example of an inerceptor being used internally is hydrating `ITaskTowerRunnerContext` so that job id's can be accessed inside a job.

### Context Parameters

You can also provide additional context when enqueuing your jobs that can be used during interception.

For example, say we usually get our user information from `HttpContext`, but since we don't have access to this when running a job, we want to make a new custom job context `ExampleJobRunnerContext` that can hold the user info for us an be accessible during a job.

Let's say we have this job that will be able to get the user form the param when passed in, but also from our example `IExampleJobRunnerContext` using DI in the context of a job.

```csharp
public class JobToDoAContextualizerThing(IExampleJobRunnerContext exampleJobRunnerContext)
{
    public sealed record Command(string? User) : IJobWithUserContext;
    
    public async Task Handle(Command request)
    {
        Log.Information("Handled JobToDoAContextualizerThing with a user from the param as: {RequestUser} and from the context as: {UserContextUser}", 
            request.User, 
            exampleJobRunnerContext?.User);
    }
}
```

First, let's make a contextualizer that can get a user from the job parameters and add it to the Task Tower `JobContext`.

```csharp
public class CurrentUserAssignmentContext : IJobContextualizer
{
    public void EnrichContext(JobContext context)
    {
        var user = jobParameters?.User;

        if(user == null)
            throw new Exception($"A User could not be established");

        context.SetJobContextParameter("User", user);
    }
}
```

Now we can make an interceptor to get this user out of context and into our `ExampleJobRunnerContext`:

```csharp
public class JobWithUserContextInterceptor : JobInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    
    public JobWithUserContextInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override JobServiceProvider Intercept(JobInterceptorContext interceptorContext)
    {
        var user = interceptorContext.GetContextParameter<string>("User");
        
        if (user == null)
        {
            return base.Intercept(interceptorContext);
        }

        var exampleJobRunnerContext = _serviceProvider.GetRequiredService<IExampleJobRunnerContext>();
        exampleJobRunnerContext.User = user;

        return new JobServiceProvider(_serviceProvider);
    }
}
```

And configure our job to user this interceptor during preprocessing so we have the user added to the service provider and accessible while the job is running:

```csharp
builder.Services.AddScoped<IExampleJobRunnerContext, ExampleJobRunnerContext>();
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.AddJobConfiguration<DoAContextualizerThing>()
        .WithPreProcessingInterceptor<JobWithUserContextInterceptor>();
  	//...
});
```

And finally, we can use that context when we enqueue our job:

```csharp
var command = new JobToDoAContextualizerThing.Command(user);
var jobId = await client
    .WithContext<JobUserAssignmentContext>()
    .Enqueue<JobToDoAContextualizerThing>(x => x.Handle(command));
```

## Accessing a Job's Id Within the Job

It's fairly common to want to access a job's id while in a job. To do this with Task Tower, you can just inject `ITaskTowerRunnerContext` and access it from there. For example:

```csharp
public class DoAnInjectableJobRunnerThing(ITaskTowerRunnerContext context)
{
    public async Task Handle()
    {
        Log.Information("I am running a job with an Id of {Id} that I got from context", context.JobId);
    }
}
```

## Benchmarks
TBD