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
    /// <summary>
    /// Adds TaskTower service to the service collection.
    /// </summary>
    public static IServiceCollection AddTaskTower(this IServiceCollection services, IConfiguration configuration, Action<TaskTowerOptions>? configureOptions = null)
    {
        // Bind TaskTowerOptions from a configuration section and apply additional configurations if provided
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

        services.AddDbContext<TaskTowerDbContext>((serviceProvider, dbOptions) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TaskTowerOptions>>().Value;
            dbOptions.UseNpgsql(options.ConnectionString, b => b.MigrationsAssembly(typeof(TaskTowerDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention();
                // .EnableSensitiveDataLogging()
                // .AddInterceptors(new ForUpdateSkipLockedInterceptor()));
        });
        services.AddHostedService<MigrationHostedService<TaskTowerDbContext>>();
        services.AddHostedService<JobNotificationListener>();

        return services;
    }
}