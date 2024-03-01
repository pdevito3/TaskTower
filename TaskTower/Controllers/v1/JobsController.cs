namespace TaskTower.Controllers.v1;

using Domain.TaskTowerJob;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Database;
using Microsoft.Extensions.Options;
using Npgsql;

[ApiController]
[Route("api/v1/jobs")]
public class JobsController(IOptions<TaskTowerOptions> options) : ControllerBase
{
    // Uncomment and use the service when ready
    // private readonly IJobService _jobService;
    //
    // public JobsController(IJobService jobService)
    // {
    //     _jobService = jobService;
    // }

    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetJobs(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        
        var jobs = await conn.QueryAsync<TaskTowerJob>($"SELECT *, job_name as JobName FROM {MigrationConfig.SchemaName}.jobs", cancellationToken);
        
        return Ok(jobs);
    }
}