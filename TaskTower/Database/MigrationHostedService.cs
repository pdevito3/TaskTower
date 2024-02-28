using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTower.Database
{
    public class MigrationHostedService : IHostedService
    {
        private readonly ILogger<MigrationHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public MigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<MigrationHostedService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Applying migrations using FluentMigrator");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                        // Validate the migrations before applying
                        runner.ListMigrations();

                        // Apply the migrations
                        runner.MigrateUp();

                        _logger.LogInformation("Migrations applied successfully using FluentMigrator");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while applying the database migrations using FluentMigrator.");
                    throw;
                }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // No operation on stop as migrations are only applied at the start
            return Task.CompletedTask;
        }
    }
}
