namespace TaskTower.Controllers.v1;

using System.Text.Json;
using Domain.TaskTowerJob;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Configurations;
using Dapper;
using Database;
using Domain.TaskTowerJob.Dtos;
using Domain.TaskTowerJob.Features;
using Domain.TaskTowerJob.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Npgsql;

[ApiController]
[Route("api/v1/jobs")]
public class JobsController(ITaskTowerJobRepository taskTowerJobRepository, IJobViewer jobViewer) : ControllerBase
{
    [HttpGet("paginated")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetPaginatedJobs([FromQuery] JobParametersDto jobParametersDto, CancellationToken cancellationToken)
    {
        var queryResponse = await taskTowerJobRepository.GetPaginatedJobs(jobParametersDto.PageNumber, 
            jobParametersDto.PageSize, 
            jobParametersDto.StatusFilter,
            jobParametersDto.QueueFilter,
            jobParametersDto.FilterText,
            cancellationToken);
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
    
    [HttpGet("queueNames")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetQueueNames()
    {
        var queueNames = await taskTowerJobRepository.GetQueueNames();
        return Ok(queueNames);
    }
    
    public sealed record BulkDeleteJobsRequest(Guid[] JobIds);
    [HttpPost("bulkDelete")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> BulkDeleteJobs([FromBody] BulkDeleteJobsRequest request)
    {
        await taskTowerJobRepository.BulkDeleteJobs(request.JobIds);
        return NoContent();
    }
    
    [HttpGet("{jobId:guid}/view")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetJobView(Guid jobId)
    {
        var jobView = await jobViewer.GetJobView(jobId);
        return Ok(jobView);
    }
    
    [HttpPut("{jobId:guid}/requeue")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> RequeueJob(Guid jobId, CancellationToken cancellationToken = default)
    {
        await taskTowerJobRepository.RequeueJob(jobId, cancellationToken);
        return NoContent();
    }
}