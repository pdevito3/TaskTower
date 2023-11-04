namespace RecipeManagement.IntegrationTests.FeatureTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RecipeManagement.Domain.Recipes.Features;

public class AddRecipeCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_recipe_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var recipeOne = new FakeRecipeForCreationDto().Generate();

        // Act
        var command = new AddRecipe.Command(recipeOne);
        var recipeReturned = await testingServiceScope.SendAsync(command);
        var recipeCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeReturned.Id));

        // Assert
        recipeReturned.Title.Should().Be(recipeOne.Title);
        recipeReturned.Directions.Should().Be(recipeOne.Directions);
        recipeReturned.RecipeSourceLink.Should().Be(recipeOne.RecipeSourceLink);
        recipeReturned.Description.Should().Be(recipeOne.Description);
        recipeReturned.ImageLink.Should().Be(recipeOne.ImageLink);
        recipeReturned.DateOfOrigin.Should().Be(recipeOne.DateOfOrigin);
        recipeReturned.Rating.Should().Be(recipeOne.Rating);
        recipeReturned.Visibility.Should().Be(recipeOne.Visibility);

        recipeCreated.Title.Should().Be(recipeOne.Title);
        recipeCreated.Directions.Should().Be(recipeOne.Directions);
        recipeCreated.RecipeSourceLink.Should().Be(recipeOne.RecipeSourceLink);
        recipeCreated.Description.Should().Be(recipeOne.Description);
        recipeCreated.ImageLink.Should().Be(recipeOne.ImageLink);
        recipeCreated.DateOfOrigin.Should().Be(recipeOne.DateOfOrigin);
        recipeCreated.Rating.Value.Should().Be(recipeOne.Rating);
        recipeCreated.Visibility.Value.Should().Be(recipeOne.Visibility);
    }
}