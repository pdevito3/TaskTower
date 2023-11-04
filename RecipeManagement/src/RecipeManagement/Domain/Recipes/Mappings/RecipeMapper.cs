namespace RecipeManagement.Domain.Recipes.Mappings;

using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.Domain.Recipes.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class RecipeMapper
{
    public static partial RecipeForCreation ToRecipeForCreation(this RecipeForCreationDto recipeForCreationDto);
    public static partial RecipeForUpdate ToRecipeForUpdate(this RecipeForUpdateDto recipeForUpdateDto);

    [MapProperty(new[] { nameof(Recipe.Rating), nameof(Recipe.Rating.Value) }, new[] { nameof(RecipeDto.Rating) })]
    public static partial RecipeDto ToRecipeDto(this Recipe recipe);

    [MapProperty(new[] { nameof(Recipe.Rating), nameof(Recipe.Rating.Value) }, new[] { nameof(RecipeDto.Rating) })]
    public static partial IQueryable<RecipeDto> ToRecipeDtoQueryable(this IQueryable<Recipe> recipe);
}
