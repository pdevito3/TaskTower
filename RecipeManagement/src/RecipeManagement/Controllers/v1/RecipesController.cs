namespace RecipeManagement.Controllers.v1;

using RecipeManagement.Domain.Recipes.Features;
using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using Domain;
using Extensions;
using MediatR;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

[ApiController]
[Route("api/recipes")]
[ApiVersion("1.0")]
public sealed class RecipesController: ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJobExecutor _jobExecutor;

    public RecipesController(IMediator mediator, IJobExecutor jobExecutor)
    {
        _mediator = mediator;
        _jobExecutor = jobExecutor;
    }


    /// <summary>
    /// Gets a list of all Recipes.
    /// </summary>
    [HttpGet(Name = "GetRecipes")]
    public async Task<IActionResult> GetRecipes([FromQuery] RecipeParametersDto recipeParametersDto)
    {
        var query = new GetRecipeList.Query(recipeParametersDto);
        var queryResponse = await _mediator.Send(query);

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

        Response.Headers.Add("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Recipe by ID.
    /// </summary>
    [HttpGet("{recipeId:guid}", Name = "GetRecipe")]
    public async Task<ActionResult<RecipeDto>> GetRecipe(Guid recipeId)
    {
        var query = new GetRecipe.Query(recipeId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Recipe record.
    /// </summary>
    [HttpPost(Name = "AddRecipe")]
    public async Task<ActionResult<RecipeDto>> AddRecipe([FromBody]RecipeForCreationDto recipeForCreation)
    {
        var command = new AddRecipe.Command(recipeForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetRecipe",
            new { recipeId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Recipe.
    /// </summary>
    [HttpPut("{recipeId:guid}", Name = "UpdateRecipe")]
    public async Task<IActionResult> UpdateRecipe(Guid recipeId, RecipeForUpdateDto recipe)
    {
        var command = new UpdateRecipe.Command(recipeId, recipe);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Recipe record.
    /// </summary>
    [HttpDelete("{recipeId:guid}", Name = "DeleteRecipe")]
    public async Task<ActionResult> DeleteRecipe(Guid recipeId)
    {
        var command = new DeleteRecipe.Command(recipeId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment

    
    [HttpPost("temp", Name = "SimpleHangfire")]
    public async Task<ActionResult> SimpleHangfire()
    {
        var jobId = await _jobExecutor
            .Enqueue<SimpleHangfireJob>(x => x.Handle());

        return Ok(jobId);
    }
    
    [HttpPost("looper", Name = "Looper")]
    public async Task<ActionResult> Looper()
    {
        var jobId = await _jobExecutor.Enqueue<LooperJob>(x => x.Handle());

        return Ok(jobId);
    }

    [HttpPost("tempVolume", Name = "SimpleHangfireVolume")]
    public async Task<ActionResult> SimpleHangfireVolume()
    {
        for (var i = 0; i < 1000; i++)
        {
            await _jobExecutor
                .Enqueue<AnotherSimpleHangfireJob>(x => x.Handle(
                    new AnotherSimpleHangfireJob.Command(i.ToString()), "i'm static"));
        }

        return NoContent();
    }
    
    [HttpPost("anothertemp/{message}", Name = "AnotherSimpleHangfire")]
    public async Task<ActionResult> AnotherSimpleHangfire(string message)
    {
        var command = new AnotherSimpleHangfireJob.Command(message);
        var jobId = await _jobExecutor
            .Enqueue<AnotherSimpleHangfireJob>(x => x.Handle(command, "i'm static"));

        return Ok(jobId);
    }
    
    // endpoint marker - do not delete this comment
}


public class AnotherSimpleHangfireJob
{
    public sealed record Command(string Message);
    //
    // [JobDisplayName("Another Simple Hangfire Job")]
    // [Queue(Consts.HangfireQueues.MySecondQueue)]
    public void Handle(Command command, string staticString)
    {
        // randomly log a message or throw an error
        var random = new Random();
        var randomNumber = random.Next(1, 10);
        if (randomNumber % 2 == 0)
        {
            Console.WriteLine($"Another Simple Hangfire Job ({command.Message}) with ({staticString}) - Success");
        }
        else
        {
            throw new Exception($"Another Simple Hangfire Job ({command.Message}) with ({staticString}) - Error");
        }
    }
}
public class SimpleHangfireJob
{
    // [JobDisplayName("Simple Hangfire Job")]
    // [Queue(Consts.HangfireQueues.MyFirstQueue)]
    public void Handle()
    {
        // randomly log a message or throw an error
        var random = new Random();
        var randomNumber = random.Next(1, 10);
        if (randomNumber % 2 == 0)
        {
            Console.WriteLine("Simple Hangfire Job - Success");
        }
        else
        {
            throw new Exception("Simple Hangfire Job - Error");
        }
    }
}

public class LooperJob
{
    private readonly IJobExecutor _jobExecutor;

    public LooperJob(IJobExecutor jobExecutor)
    {
        _jobExecutor = jobExecutor;
    }

    public async Task Handle()
    {
        for (var i = 0; i < 1000; i++)
        {
            await _jobExecutor
                .Enqueue<SimpleHangfireJob>(x => x.Handle());
        }
    }
}

