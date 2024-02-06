namespace TaskTowerSandbox.Configurations;

using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Processing;

public delegate void ConfigureTaskTowerOptions(IServiceProvider serviceProvider, TaskTowerOptions options);

public static class TaskTowerServiceRegistration
{
    /// <summary>
    /// Adds TaskTower service to the service collection.
    /// </summary>
    public static IServiceCollection AddTaskTower(this IServiceCollection services, Action<TaskTowerOptions>? configure = null)
    {
        // services.Configure<TaskTowerOptions>(configuration.GetSection("TaskTowerOptions"));
        var options = new TaskTowerOptions();
        configure?.Invoke(options);
        
        services.AddDbContext<TaskTowerDbContext>(dbOptions =>
            dbOptions.UseNpgsql(options.ConnectionString, b => b.MigrationsAssembly(typeof(TaskTowerDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention());
                // .EnableSensitiveDataLogging()
                // .AddInterceptors(new ForUpdateSkipLockedInterceptor()));
        services.AddHostedService<MigrationHostedService<TaskTowerDbContext>>();
        services.AddHostedService<JobNotificationListener>();

        return services;
    }
}