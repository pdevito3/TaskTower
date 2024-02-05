using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TaskTowerSandbox;
using TaskTowerSandbox.Database;
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

builder.Services.AddDbContext<TaskTowerDbContext>(options =>
    options.UseNpgsql(Consts.ConnectionString,
            b => b.MigrationsAssembly(typeof(TaskTowerDbContext).Assembly.FullName))
        .UseSnakeCaseNamingConvention()
        // .EnableSensitiveDataLogging()
        .AddInterceptors(new ForUpdateSkipLockedInterceptor()));
builder.Services.AddHostedService<MigrationHostedService<TaskTowerDbContext>>();
builder.Services.AddHostedService<JobNotificationListener>();

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

app.MapPost("/create-many-jobs", async (HttpContext http, TaskTowerDbContext context) =>
{
    try
    {
        for (var i = 0; i < 100; i++)
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

app.MapPost("/five-second-delay", async (HttpContext http, TaskTowerDbContext context) =>
{
    var jobForCreation = new TaskTowerJobForCreation()
    {
        Queue = Guid.NewGuid().ToString(),
        Payload = JsonSerializer.Serialize(Guid.NewGuid()),
        RunAfter = DateTimeOffset.UtcNow.AddSeconds(5)
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

app.Run();


public record JobData(string Payload);
