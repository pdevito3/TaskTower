using System.Text.Json;
using Dapper;
using HelloPgPubSub;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RecipeManagement.Databases;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(Consts.ConnectionString,
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
        .UseSnakeCaseNamingConvention());
builder.Services.AddHostedService<MigrationHostedService<ApplicationDbContext>>();
builder.Services.AddHostedService<JobNotificationListener>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/create-job", async (JobData request, HttpContext http, ApplicationDbContext context) =>
{
    if (string.IsNullOrWhiteSpace(request.Payload))
    {
        return Results.BadRequest("Invalid job payload.");
    }

    var job = new Job
    {
        Status = "pending",
        Payload = JsonSerializer.Serialize(request)
    };

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

app.MapPost("/create-many-jobs", async (HttpContext http, ApplicationDbContext context) =>
{
    try
    {
        for (var i = 0; i < 100; i++)
        {
            var job = new Job
            {
                Status = "pending",
                Payload = JsonSerializer.Serialize(Guid.NewGuid())
            };
            context.Jobs.Add(job);
            // Log.Information("Job in EF with ID: {Id}", job.Id);
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

app.Run();
