namespace RecipeManagement.IntegrationTests.FeatureTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using RecipeManagement.Domain.Recipes.Dtos;
using RecipeManagement.Domain.Recipes.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateRecipeCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_recipe_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var recipe = new FakeRecipeBuilder().Build();
        await testingServiceScope.InsertAsync(recipe);
        var updatedRecipeDto = new FakeRecipeForUpdateDto().Generate();

        // Act
        var command = new UpdateRecipe.Command(recipe.Id, updatedRecipeDto);
        await testingServiceScope.SendAsync(command);
        var updatedRecipe = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Recipes
                .FirstOrDefaultAsync(r => r.Id == recipe.Id));

        // Assert
        updatedRecipe.Title.Should().Be(updatedRecipeDto.Title);
        updatedRecipe.Directions.Should().Be(updatedRecipeDto.Directions);
        updatedRecipe.RecipeSourceLink.Should().Be(updatedRecipeDto.RecipeSourceLink);
        updatedRecipe.Description.Should().Be(updatedRecipeDto.Description);
        updatedRecipe.ImageLink.Should().Be(updatedRecipeDto.ImageLink);
        updatedRecipe.DateOfOrigin.Should().Be(updatedRecipeDto.DateOfOrigin);
        updatedRecipe.Rating.Value.Should().Be(updatedRecipeDto.Rating);
        updatedRecipe.Visibility.Value.Should().Be(updatedRecipeDto.Visibility);
    }
}