namespace RecipeManagement.Controllers.v1;

using RecipeManagement.Domain.Recipes.Features;
using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.Wrappers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using MediatR;

[ApiController]
[Route("api/recipes")]
[ApiVersion("1.0")]
public sealed class RecipesController: ControllerBase
{
    private readonly IMediator _mediator;

    public RecipesController(IMediator mediator)
    {
        _mediator = mediator;
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
}
