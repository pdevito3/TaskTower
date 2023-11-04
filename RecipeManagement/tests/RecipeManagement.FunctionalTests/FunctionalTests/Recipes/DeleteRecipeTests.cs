namespace RecipeManagement.FunctionalTests.FunctionalTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using RecipeManagement.FunctionalTests.TestUtilities;
using System.Net;
using System.Threading.Tasks;

public class DeleteRecipeTests : TestBase
{
    [Fact]
    public async Task delete_recipe_returns_nocontent_when_entity_exists()
    {
        // Arrange
        var recipe = new FakeRecipeBuilder().Build();
        await InsertAsync(recipe);

        // Act
        var route = ApiRoutes.Recipes.Delete(recipe.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}