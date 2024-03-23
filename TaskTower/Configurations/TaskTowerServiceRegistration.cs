namespace TaskTower.Configurations;

using Controllers.v1;
using Dapper;
using Database;
using Domain.JobStatuses;
using Domain.JobStatuses.Mappings;
using Domain.RunHistories.Services;
using Domain.TaskTowerJob.Features;
using Domain.TaskTowerJob.Services;
using FluentMigrator.Runner;
using Interception;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Processing;

public delegate void ConfigureTaskTowerOptions(IServiceProvider serviceProvider, TaskTowerOptions options);

public static class TaskTowerServiceRegistration
{
    /// <summary>
    /// Adds TaskTower service to the service collection.
    /// </summary>
    public static IServiceCollection AddTaskTower(this IServiceCollection services, IConfiguration configuration,
        Action<TaskTowerOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure<TaskTowerOptions>(options =>
            {
                configuration.GetSection("TaskTowerOptions").Bind(options);
                configureOptions(options);
            });
        }
        else
        {
            services.Configure<TaskTowerOptions>(configuration.GetSection("TaskTowerOptions"));
        }

        var options = configuration.GetSection("TaskTowerOptions").Get<TaskTowerOptions>();
        if (configureOptions != null)
        {
            var tempOptions = new TaskTowerOptions();
            configuration.GetSection("TaskTowerOptions").Bind(tempOptions);
            configureOptions(tempOptions);
            options = tempOptions;
        }

        SqlMapper.AddTypeHandler(typeof(JobStatus), new JobStatusTypeHandler());
        
        services.AddScoped<ITaskTowerJobRepository, TaskTowerJobRepository>();
        services.AddScoped<IJobRunHistoryRepository, JobRunHistoryRepository>();
        services.AddScoped<IJobViewer, JobViewer>();
        services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
        services.AddScoped<ITaskTowerRunnerContext, TaskTowerRunnerContext>();

        MigrationConfig.SchemaName = options!.Schema;
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(options!.ConnectionString)
                .ScanIn(typeof(TaskTowerServiceRegistration).Assembly).For.Migrations())
            // .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        services.AddCorsService("TaskTowerCorsPolicy");
        services.AddHostedService<MigrationHostedService>();
        services.AddHostedService<JobNotificationListener>();

        services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(typeof(JobsController).Assembly));

        return services;
    }
}