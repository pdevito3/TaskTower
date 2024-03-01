namespace TaskTower.Controllers.v1;

using Domain.TaskTowerJob;
using Domain.TaskTowerJob.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/v1/jobs")] // Updated to reflect versioning in the route
public class JobsController : ControllerBase
{
    // Uncomment and use the service when ready
    // private readonly IJobService _jobService;
    //
    // public JobsController(IJobService jobService)
    // {
    //     _jobService = jobService;
    // }

    [HttpGet]
    public async Task<IActionResult> GetJobs()
    {
        var dummyJobData = new List<TaskTowerJob>
        {
            TaskTowerJob.Create(new TaskTowerJobForCreation() { }),
            TaskTowerJob.Create(new TaskTowerJobForCreation() { }),
            TaskTowerJob.Create(new TaskTowerJobForCreation() { }),
        };
        
        return Ok(dummyJobData);
    }
}