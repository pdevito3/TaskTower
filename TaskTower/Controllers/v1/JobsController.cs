namespace TaskTower.Controllers.v1;

using Domain.TaskTowerJob;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Database;
using Domain.TaskTowerJob.Services;
using Microsoft.Extensions.Options;
using Npgsql;

[ApiController]
[Route("api/v1/jobs")]
public class JobsController(ITaskTowerJobRepository taskTowerJobRepository) : ControllerBase
{

    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetJobs(CancellationToken cancellationToken)
    {
        var jobs = await taskTowerJobRepository.GetJobs(cancellationToken);
        return Ok(jobs);
    }
    
    // get paginated
    [HttpGet("paginated")]
    // [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetPaginatedJobs(int? page, int? pageSize, CancellationToken cancellationToken)
    {
        page ??= 1;
        pageSize ??= 10;
        var jobs = await taskTowerJobRepository.GetPaginatedJobs((int)page, (int)pageSize, cancellationToken);
        var dto = jobs.Select(x => new
        {
            Id = x.Id,
            Status = x.Status.Value,
            JobName = x.JobName,
            Retries = x.Retries,
            MaxRetries = x.MaxRetries ,
            RunAfter = x.RunAfter,
            Deadline = x.Deadline,
            CreatedAt = x.CreatedAt,
            Type = x.Type,
            Method = x.Method,
            ParameterTypes = x.ParameterTypes ?? Array.Empty<string>(),
            Payload = x.Payload,
            Queue = x.Queue,
            RanAt = x.RanAt,
        });
        return Ok(dto);
    }
}