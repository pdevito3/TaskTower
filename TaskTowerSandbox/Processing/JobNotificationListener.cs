namespace TaskTowerSandbox.Processing;

using System.Threading;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using TaskTowerSandbox.Domain.JobStatuses;
using TaskTowerSandbox.Domain.TaskTowerJob;

public class JobNotificationListener(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(Consts.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        await using (var cmd = new NpgsqlCommand("LISTEN job_available", conn))
        {
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }

        conn.Notification += async (_, e) =>
        {
            Log.Information("Notification received: Job available with ID {JobId}", e.Payload);
            await ProcessJob(stoppingToken);
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            // This call is blocking until a notification is received
            await conn.WaitAsync(stoppingToken);
        }
    }

    private async Task ProcessJob(CancellationToken stoppingToken)
    {
        // get TaskTowerOptions from Configuration
        var options = serviceScopeFactory.CreateScope()
            .ServiceProvider.GetRequiredService<IOptions<TaskTowerOptions>>().Value;
        Log.Information("Processing job with options: {@Options}", options);
        
        await using var conn = new NpgsqlConnection(Consts.ConnectionString);
        await conn.OpenAsync(stoppingToken);
        
        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        
        // Fetch the next available job that is not already locked by another process
        var job = await conn.QueryFirstOrDefaultAsync<TaskTowerJob>(
            $@"
                SELECT id, payload 
                FROM jobs 
                WHERE status = '{JobStatus.Pending().Value}' 
                ORDER BY created_at 
                FOR UPDATE SKIP LOCKED 
                LIMIT 1",
            transaction: tx
        );
        
        if (job != null)
        {
            Log.Information($"Processing job {job.Id} with payload {job.Payload}");
            // Process the job here
            var updateResult = await conn.ExecuteAsync(
                $"UPDATE jobs SET status = '{JobStatus.Completed().Value}' WHERE id = @Id",
                new { job.Id },
                transaction: tx
            );
        }
        
        await tx.CommitAsync(stoppingToken);
    }
}

