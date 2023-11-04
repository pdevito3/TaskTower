namespace RecipeManagement.SharedTestHelpers.Fakes.Recipe;

using AutoBogus;
using RecipeManagement.Domain.Recipes;
using RecipeManagement.Domain.Recipes.Models;

public sealed class FakeRecipeForUpdate : AutoFaker<RecipeForUpdate>
{
    public FakeRecipeForUpdate()
    {
        RuleFor(r => r.Visibility, f => f.PickRandom<VisibilityEnum>(VisibilityEnum.List).Name);
    }
}