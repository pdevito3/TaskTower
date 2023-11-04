namespace RecipeManagement.FunctionalTests.FunctionalTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using RecipeManagement.FunctionalTests.TestUtilities;
using System.Net;
using System.Threading.Tasks;

public class CreateRecipeTests : TestBase
{
    [Fact]
    public async Task create_recipe_returns_created_using_valid_dto()
    {
        // Arrange
        var recipe = new FakeRecipeForCreationDto().Generate();

        // Act
        var route = ApiRoutes.Recipes.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, recipe);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}