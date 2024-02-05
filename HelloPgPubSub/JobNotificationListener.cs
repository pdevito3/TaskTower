namespace HelloPgPubSub;

using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

// public class JobNotificationListener : BackgroundService
// {
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         await using var conn = new NpgsqlConnection(Consts.ConnectionString);
//         await conn.OpenAsync(stoppingToken);
//
//         await using (var cmd = new NpgsqlCommand("LISTEN job_available", conn))
//         {
//             await cmd.ExecuteNonQueryAsync(stoppingToken);
//         }
//
//         conn.Notification += (_, e) =>
//         {
//             // Console.WriteLine($"Notification received: Job available with ID {e.Payload}");
//             Log.Information("Notification received: Job available with ID {JobId}", e.Payload);
//             
//         };
//
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             await conn.WaitAsync(stoppingToken);
//         }
//     }
// }

using Dapper;

public class JobNotificationListener : BackgroundService
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
        await using var conn = new NpgsqlConnection(Consts.ConnectionString);
        await conn.OpenAsync(stoppingToken);

        await using var tx = await conn.BeginTransactionAsync(stoppingToken);
        // Fetch the next available job that is not already locked by another process
        var job = await conn.QueryFirstOrDefaultAsync<Job>(
            @"
                SELECT id, payload 
                FROM jobs 
                WHERE status = 'pending' 
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
                "UPDATE jobs SET status = 'completed' WHERE id = @Id",
                new { job.Id },
                transaction: tx
            );
        }

        await tx.CommitAsync(stoppingToken);
    }
}

