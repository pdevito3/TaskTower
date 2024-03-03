namespace TaskTower.Controllers.v1;

using System.Text.Json;
using Domain.TaskTowerJob;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Database;
using Domain.TaskTowerJob.Services;
using Microsoft.AspNetCore.Http;
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
    
    [HttpGet("paginated")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetPaginatedJobs(int? pageNumber, int? pageSize, CancellationToken cancellationToken)
    {
        pageNumber ??= 1;
        pageSize ??= 10;
        var queryResponse = await taskTowerJobRepository.GetPaginatedJobs((int)pageNumber, (int)pageSize, cancellationToken);
        var dto = queryResponse.Select(x => new
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
        
        var paginationMetadata = new
        {
            totalCount = queryResponse.TotalCount,
            pageSize = queryResponse.PageSize,
            currentPageSize = queryResponse.CurrentPageSize,
            currentStartIndex = queryResponse.CurrentStartIndex,
            currentEndIndex = queryResponse.CurrentEndIndex,
            pageNumber = queryResponse.PageNumber,
            totalPages = queryResponse.TotalPages,
            hasPrevious = queryResponse.HasPrevious,
            hasNext = queryResponse.HasNext
        };
        
        Response.Headers.Append("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));
        
        return Ok(dto);
    }
}