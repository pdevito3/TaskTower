namespace RecipeManagement.FunctionalTests.FunctionalTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using RecipeManagement.FunctionalTests.TestUtilities;
using System.Net;
using System.Threading.Tasks;

public class GetRecipeListTests : TestBase
{
    [Fact]
    public async Task get_recipe_list_returns_success()
    {
        // Arrange
        

        // Act
        var result = await FactoryClient.GetRequestAsync(ApiRoutes.Recipes.GetList);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}