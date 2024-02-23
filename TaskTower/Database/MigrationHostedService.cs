namespace TaskTower.Database;

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

public class MigrationHostedService<TDbContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<MigrationHostedService<TDbContext>> logger)
    : IHostedService
    where TDbContext : DbContext
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Applying migrations for {DbContext}", typeof(TDbContext).Name);

            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await context.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Migrations complete for {DbContext}", typeof(TDbContext).Name);
        }
        catch (Exception ex) when (ex is SocketException or NpgsqlException)
        {
            logger.LogError(ex, "Could not connect to the database. Please check the connection string and make sure the database is running.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying the database migrations.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}