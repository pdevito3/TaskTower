namespace RecipeManagement.FunctionalTests.FunctionalTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using RecipeManagement.FunctionalTests.TestUtilities;
using System.Net;
using System.Threading.Tasks;

public class GetRecipeTests : TestBase
{
    [Fact]
    public async Task get_recipe_returns_success_when_entity_exists()
    {
        // Arrange
        var recipe = new FakeRecipeBuilder().Build();
        await InsertAsync(recipe);

        // Act
        var route = ApiRoutes.Recipes.GetRecord(recipe.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}