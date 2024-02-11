using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TaskTowerSandbox;
using TaskTowerSandbox.Configurations;
using TaskTowerSandbox.Database;
using TaskTowerSandbox.Domain.QueuePrioritizationes;
using TaskTowerSandbox.Domain.TaskTowerJob;
using TaskTowerSandbox.Domain.TaskTowerJob.Models;
using TaskTowerSandbox.Processing;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
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

builder.Services.AddTaskTower(builder.Configuration,x =>
{
    x.ConnectionString = Consts.ConnectionString;
    x.QueuePriorities = new Dictionary<string, int>
    {
        {"high", 3},
        {"default", 2},
        {"low", 1}
    };
    x.QueuePrioritization = QueuePrioritization.Strict();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();


app.MapPost("/create-job", async (JobData request, HttpContext http, TaskTowerDbContext context) =>
{
    if (string.IsNullOrWhiteSpace(request.Payload))
    {
        return Results.BadRequest("Invalid job payload.");
    }

    var jobForCreation = new TaskTowerJobForCreation()
    {
        Queue = Guid.NewGuid().ToString(),
        Payload = JsonSerializer.Serialize(request)
    };
    var job = TaskTowerJob.Create(jobForCreation);

    try
    {
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        return Results.Ok(new { Message = $"Job created with ID: {job.Id}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-a-few-jobs", async (HttpContext http, TaskTowerDbContext context) =>
{
    try
    {
        for (var i = 0; i < 5; i++)
        {
            var jobForCreation = new TaskTowerJobForCreation()
            {
                // Queue = Guid.NewGuid().ToString(),
                Queue = "default",
                Payload = JsonSerializer.Serialize(Guid.NewGuid())
            };
            var job = TaskTowerJob.Create(jobForCreation);
            context.Jobs.Add(job);
        }
        
        await context.SaveChangesAsync();
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-many-jobs", async (HttpContext http, TaskTowerDbContext context) =>
{
    try
    {
        for (var i = 0; i < 500; i++)
        {
            var jobForCreation = new TaskTowerJobForCreation()
            {
                Queue = "default",
                Payload = JsonSerializer.Serialize(Guid.NewGuid())
            };
            var job = TaskTowerJob.Create(jobForCreation);
            context.Jobs.Add(job);
        }
        
        await context.SaveChangesAsync();
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/create-many-many-jobs", async (HttpContext http, TaskTowerDbContext context) =>
{
    try
    {
        for (var i = 0; i < 10000; i++)
        {
            var jobForCreation = new TaskTowerJobForCreation()
            {
                Queue = Guid.NewGuid().ToString(),
                Payload = JsonSerializer.Serialize(Guid.NewGuid())
            };
            var job = TaskTowerJob.Create(jobForCreation);
            context.Jobs.Add(job);
        }
        
        await context.SaveChangesAsync();
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/two-second-delay", async (HttpContext http, TaskTowerDbContext context) =>
{
    var jobForCreation = new TaskTowerJobForCreation()
    {
        Queue = Guid.NewGuid().ToString(),
        Payload = JsonSerializer.Serialize(Guid.NewGuid()),
        RunAfter = DateTimeOffset.UtcNow.AddSeconds(2)
    };
    var job = TaskTowerJob.Create(jobForCreation);

    try
    {
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        return Results.Ok(new { Message = $"Job created with ID: {job.Id}" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/many-2-second-delay", async (HttpContext http, TaskTowerDbContext context) =>
{
    try
    {
        for (var i = 0; i < 5000; i++)
        {
            var jobForCreation = new TaskTowerJobForCreation()
            {
                Queue = Guid.NewGuid().ToString(),
                Payload = JsonSerializer.Serialize(Guid.NewGuid()),
                RunAfter = DateTimeOffset.UtcNow.AddSeconds(2)
            };
            var job = TaskTowerJob.Create(jobForCreation);
            context.Jobs.Add(job);
        }
        
        await context.SaveChangesAsync();
        return Results.Ok(new { Message = $"Jobs created" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});

app.MapPost("/queued-test", async (HttpContext http, TaskTowerDbContext context) =>
{

    var highJob = TaskTowerJob.Create(new TaskTowerJobForCreation()
    {
        Queue = "high",
        Payload = JsonSerializer.Serialize("this is a high job")
    });
    var defaultJob = TaskTowerJob.Create(new TaskTowerJobForCreation()
    {
        Queue = "default",
        Payload = JsonSerializer.Serialize("this is a default job")
    });
    var lowJob = TaskTowerJob.Create(new TaskTowerJobForCreation()
    {
        Queue = "low",
        Payload = JsonSerializer.Serialize("this is a low job")
    });

    try
    {
        context.Jobs.Add(highJob);
        context.Jobs.Add(defaultJob);
        context.Jobs.Add(lowJob);
        await context.SaveChangesAsync();

        return Results.Ok(new { Message = $"queued jobs added" });
    }
    catch (Exception ex)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating job: {Message}", ex.Message);
        return Results.Problem("An error occurred while creating the job.");
    }
});


app.MapPost("/large-queued-test", async (HttpContext http, TaskTowerDbContext context) =>
{
    try
    {
        for (var i = 0; i < 10; i++)
        {
            var highJob = TaskTowerJob.Create(new TaskTowerJobForCreation()
            {
                Queue = "high",
                Payload = JsonSerializer.Serialize("this is a high job")
            });

            context.Jobs.Add(highJob);
        }
        for (var i = 0; i < 5; i++)
        {
            var defaultJob = TaskTowerJob.Create(new TaskTowerJobForCreation()
            {
                Queue = "default",
                Payload = JsonSerializer.Serialize("this is a default job")
            });
            context.Jobs.Add(defaultJob);
        }
        
        for (var i = 0; i < 3; i++)
        {
            var lowJob = TaskTowerJob.Create(new TaskTowerJobForCreation()
            {
                Queue = "low",
                Payload = JsonSerializer.Serialize("this is a low job")
            });
            context.Jobs.Add(lowJob);
        }
        
        await context.SaveChangesAsync();
        return Results.Ok(new { Message = $"queued jobs added" });
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
