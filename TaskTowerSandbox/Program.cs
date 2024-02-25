using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TaskTower;
using TaskTower.Configurations;
using TaskTower.Domain.QueuePrioritizations;
using TaskTower.Processing;
using TaskTowerSandbox;
using TaskTowerSandbox.Sandboxing;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("TaskTower", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
    .Enrich.FromLogContext()
    // .Destructure.UsingAttributes()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateLogger();
builder.Host.UseSerilog();

var serverName = Environment.MachineName;
Log.Information("Starting {Application} on {MachineName}", builder.Environment.ApplicationName, serverName);

builder.Services.AddHttpClient("PokeAPI", client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
});
builder.Services.AddScoped<PokeApiService>();

builder.Services.AddScoped<IJobContextAccessor, JobContextAccessor>();
builder.Services.AddScoped<IJobWithUserContext, JobWithUserContext>();
builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.ConnectionString = TaskTowerConstants.ConnectionString;
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"critical", 3},
        {"default", 2},
        {"low", 1}
        
        // {"high", 60},
        // {"default", 30},
        // {"low", 10}
    };
    x.QueuePrioritization = QueuePrioritization.None();
    // x.IdleTransactionTimeout = 1000;
    
    x.AddJobConfiguration<DoAPossiblyFailingThing>(x =>
    {
        x.Queue = "critical";
        x.DisplayName = "Possibly Failing Task";
        x.MaxRetryCount = 2;
    });
    x.AddJobConfiguration<DoACriticalThing>(x =>
    {
        x.Queue = "critical";
        x.DisplayName = "Critical Task";
        x.MaxRetryCount = 5;
    });
    x.AddJobConfiguration<DoALowThing>(x =>
    {
        x.Queue = "low";
        x.DisplayName = "Low Task";
        x.MaxRetryCount = 1;
    });
    // x.AddJobConfiguration<DoAMiddlewareThing>(x =>
    // {
    //     x.WithActivator<JobWithUserContextActivator>(builder?.Services);
    // });
});

builder.Services.AddScoped<IDummyLogger, DummyLogger>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();



app.MapPost("/create-middleware-job", async (string user, HttpContext http, IBackgroundJobClient client) =>
{
    if (string.IsNullOrWhiteSpace(user))
    {
        return Results.BadRequest("Invalid job payload.");
    }

    try
    {
        var command = new DoAMiddlewareThing.Command(user);
        var jobId = await client
            .WithCreationMiddleware<JobUserMiddlewareWasAttribute>()
            .Enqueue<DoAMiddlewareThing>(x => x.Handle(command));

        return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/console-log", async (JobData request, HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        var jobId = await client.Enqueue(() => Console.WriteLine("this is simple"));
        return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

// TODO this is a broken flow
// app.MapPost("/async-simple-log", async (JobData request, HttpContext http, IBackgroundJobClient client) =>
// {
//     try
//     {
//         if (string.IsNullOrWhiteSpace(request.Payload))
//         {
//             return Results.BadRequest("Invalid job payload.");
//         }
//         
//         var jobId = await client.Enqueue(() => Task.Run(() => Console.WriteLine($"Async simple log - {request.Payload}")));
//         return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
//     }
//     catch (Exception ex)
//     {
//         var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "Error creating job: {Message}", ex.Message);
//         return Results.Problem("An error occurred while creating the job.");
//     }
// });

app.MapPost("/create-sync-job", async (JobData request, HttpContext http, IBackgroundJobClient client) =>
{
    if (string.IsNullOrWhiteSpace(request.Payload))
    {
        return Results.BadRequest("Invalid job payload.");
    }

    try
    {
        var command = new DoASynchronousThing.Command(request.Payload);
        var jobId = await client.Enqueue<DoASynchronousThing>(x => x.Handle(command));

        return Results.Ok(new { Message = $"Synchronous job created with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating synchronous job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the synchronous job.");
    }
});

app.MapPost("/create-injectable-job", async (JobData request, HttpContext http, IBackgroundJobClient client) =>
{
    if (string.IsNullOrWhiteSpace(request.Payload))
    {
        return Results.BadRequest("Invalid job payload.");
    }

    try
    {
        var command = new DoAnInjectableThing.Command(request.Payload);
        var jobId = await client.Enqueue<DoAnInjectableThing>(x => x.Handle(command));

        return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-many-injectable-jobs", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        for (var i = 0; i < 10; i++)
        {
            var command = new DoAnInjectableThing.Command(Guid.NewGuid().ToString());
            await client.Enqueue<DoAnInjectableThing>(x => x.Handle(command));
        }
        
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-job", async (JobData request, HttpContext http, IBackgroundJobClient client) =>
{
    if (string.IsNullOrWhiteSpace(request.Payload))
    {
        return Results.BadRequest("Invalid job payload.");
    }

    try
    {
        var command = new DoAThing.Command(request.Payload);
        var jobId = await client.Enqueue<DoAThing>(x => x.Handle(command));

        return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-a-few-jobs", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        for (var i = 0; i < 5; i++)
        {
            var command = new DoAThing.Command(Guid.NewGuid().ToString());
            await client.Enqueue<DoAThing>(x => x.Handle(command));
        }
        
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-many-jobs", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        for (var i = 0; i < 500; i++)
        {
            var command = new DoAThing.Command(Guid.NewGuid().ToString());
            await client.Enqueue<DoAThing>(x => x.Handle(command));
        }
        
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-many-many-jobs", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        for (var i = 0; i < 10000; i++)
        {
            var command = new DoAThing.Command(Guid.NewGuid().ToString());
            await client.Enqueue<DoAThing>(x => x.Handle(command));
        }
        
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/two-second-delay", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        var jobId = await client.Schedule<DoAThing>(x => 
            x.Handle(new DoAThing.Command("this is a scheduled job")), 
            TimeSpan.FromSeconds(2));

        return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/fluent-two-second-delay", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        var jobId = await client.Schedule<DoAThing>(x => 
                x.Handle(new DoAThing.Command("this is a scheduled job")))
            .ToQueue("critical")
            .InSeconds(2);

        return Results.Ok(new { Message = $"Job created with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/many-2-second-delay", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        for (var i = 0; i < 5000; i++)
        {
            await client.Schedule<DoAThing>(x => 
                x.Handle(new DoAThing.Command("this is a scheduled job")), 
                TimeSpan.FromSeconds(2));
        }
        
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/queued-test", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        var criticalCommand = new DoACriticalThing.Command("this is a critical job");
        await client.Enqueue<DoACriticalThing>(x => x.Handle(criticalCommand));
    
        var defaultCommand = new DoADefaultThing.Command("this is a default job");
        await client.Enqueue<DoADefaultThing>(x => x.Handle(defaultCommand));
    
        var lowCommand = new DoALowThing.Command("this is a low job");
        await client.Enqueue<DoALowThing>(x => x.Handle(lowCommand));

        return Results.Ok(new { Message = $"queued jobs added" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});


app.MapPost("/large-queued-test-scheduled", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        for (var i = 0; i < 300; i++)
        {
            var lowCommand = new DoALowThing.Command("this is a low job");
            await client.Schedule<DoALowThing>(x => x.Handle(lowCommand), 
                TimeSpan.FromSeconds(2));
        }
        for (var i = 0; i < 100; i++)
        {
            var criticalCommand = new DoACriticalThing.Command("this is a critical job");
            await client.Schedule<DoACriticalThing>(x => x.Handle(criticalCommand), 
                TimeSpan.FromSeconds(2));
        }
        for (var i = 0; i < 100; i++)
        {
            var defaultCommand = new DoADefaultThing.Command("this is a default job");
            await client.Schedule<DoADefaultThing>(x => x.Handle(defaultCommand), 
                TimeSpan.FromSeconds(2));
        }
        
        for (var i = 0; i < 100; i++)
        {
            var criticalCommand = new DoACriticalThing.Command("this is a critical job");
            await client.Schedule<DoACriticalThing>(x => x.Handle(criticalCommand), 
                TimeSpan.FromSeconds(2));
        }
        
        for (var i = 0; i < 300; i++)
        {
            var lowCommand = new DoALowThing.Command("this is a low job");
            await client.Schedule<DoALowThing>(x => x.Handle(lowCommand), 
                TimeSpan.FromSeconds(2));
        }
        
        return Results.Ok(new { Message = $"queued jobs added" });
    }

    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/large-queued-test-immediate", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        for (var i = 0; i < 300; i++)
        {
            var lowCommand = new DoALowThing.Command("this is a low job");
            await client.Enqueue<DoALowThing>(x => x.Handle(lowCommand));
        }
        for (var i = 0; i < 100; i++)
        {
            var criticalCommand = new DoACriticalThing.Command("this is a critical job");
            await client.Enqueue<DoACriticalThing>(x => x.Handle(criticalCommand));
        }
        for (var i = 0; i < 100; i++)
        {
            var defaultCommand = new DoADefaultThing.Command("this is a default job");
            await client.Enqueue<DoADefaultThing>(x => x.Handle(defaultCommand));
        }
        
        for (var i = 0; i < 100; i++)
        {
            var criticalCommand = new DoACriticalThing.Command("this is a critical job");
            await client.Enqueue<DoACriticalThing>(x => x.Handle(criticalCommand));
        }
        
        for (var i = 0; i < 300; i++)
        {
            var lowCommand = new DoALowThing.Command("this is a low job");
            await client.Enqueue<DoALowThing>(x => x.Handle(lowCommand));
        }
        
        return Results.Ok(new { Message = $"queued jobs added" });
    }

    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});


app.MapPost("/can-fail", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        var command = new DoAPossiblyFailingThing.Command("fail");
        await client.Enqueue<DoAPossiblyFailingThing>(x => x.Handle(command));

        return Results.Ok(new { Message = $"queued job added" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});


app.MapPost("/do-a-slow-thing", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        var command = new DoASlowThing.Command("this is a slow job");
        await client.Enqueue<DoASlowThing>(x => x.Handle(command));

        return Results.Ok(new { Message = $"queued job added" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});


app.MapPost("/do-a-job-with-a-few-tags", async (HttpContext http, IBackgroundJobClient client) =>
{
    try
    {
        var command = new DoAThing.Command("this is a tagged job");
        var jobId = await client.Enqueue<DoAThing>(x => x.Handle(command));
        client.TagJob(jobId, "tag1")
            .TagJob(jobId, "tag2");
        
        await client.TagJobAsync(jobId, "tag3");
        await client.TagJobAsync(jobId,  ["tag3", "tag4"]);
        
        client.TagJob(jobId, "tag4", "tag5", "tag6");
        client.TagJob(jobId, ["tag7", "tag8", "tag9"]);
        client.TagJob(jobId,  ["tag10", "tag11"]);

        return Results.Ok(new { Message = $"queued job added with ID: {jobId}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.Run();


public record JobData(string Payload);
