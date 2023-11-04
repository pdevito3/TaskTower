namespace RecipeManagement.Domain.Recipes.Features;

using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.Domain.Recipes.Services;
using RecipeManagement.Exceptions;
using Mappings;
using MediatR;

public static class GetRecipe
{
    public sealed record Query(Guid RecipeId) : IRequest<RecipeDto>;

    public sealed class Handler : IRequestHandler<Query, RecipeDto>
    {
        private readonly IRecipeRepository _recipeRepository;

        public Handler(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        public async Task<RecipeDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await _recipeRepository.GetById(request.RecipeId, cancellationToken: cancellationToken);
            return result.ToRecipeDto();
        }
    }
}