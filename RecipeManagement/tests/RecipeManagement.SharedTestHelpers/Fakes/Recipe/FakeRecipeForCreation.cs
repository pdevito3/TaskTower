namespace RecipeManagement.SharedTestHelpers.Fakes.Recipe;

using AutoBogus;
using RecipeManagement.Domain.Recipes;
using RecipeManagement.Domain.Recipes.Models;

public sealed class FakeRecipeForCreation : AutoFaker<RecipeForCreation>
{
    public FakeRecipeForCreation()
    {
        RuleFor(r => r.Visibility, f => f.PickRandom<VisibilityEnum>(VisibilityEnum.List).Name);
    }
}