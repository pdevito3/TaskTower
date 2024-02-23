namespace TaskTower.Database;

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;

public class MigrationHostedService<TDbContext>(
    IServiceScopeFactory scopeFactory)
    : IHostedService
    where TDbContext : DbContext
{
    private readonly ILogger _logger = Log.ForContext<MigrationHostedService<TDbContext>>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("Applying migrations for {DbContext}", typeof(TDbContext).Name);

            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await context.Database.MigrateAsync(cancellationToken);

            _logger.Information("Migrations complete for {DbContext}", typeof(TDbContext).Name);
        }
        catch (Exception ex) when (ex is SocketException or NpgsqlException)
        {
            _logger.Error(ex, "Could not connect to the database. Please check the connection string and make sure the database is running.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while applying the database migrations.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}