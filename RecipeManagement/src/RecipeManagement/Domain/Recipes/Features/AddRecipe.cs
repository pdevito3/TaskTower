namespace RecipeManagement.Domain.Recipes.Features;

using RecipeManagement.Domain.Recipes.Services;
using RecipeManagement.Domain.Recipes;
using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.Domain.Recipes.Models;
using RecipeManagement.Services;
using RecipeManagement.Exceptions;
using Mappings;
using MediatR;

public static class AddRecipe
{
    public sealed record Command(RecipeForCreationDto RecipeToAdd) : IRequest<RecipeDto>;

    public sealed class Handler : IRequestHandler<Command, RecipeDto>
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IRecipeRepository recipeRepository, IUnitOfWork unitOfWork)
        {
            _recipeRepository = recipeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RecipeDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var recipeToAdd = request.RecipeToAdd.ToRecipeForCreation();
            var recipe = Recipe.Create(recipeToAdd);

            await _recipeRepository.Add(recipe, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return recipe.ToRecipeDto();
        }
    }
}