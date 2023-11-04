namespace RecipeManagement.FunctionalTests.FunctionalTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using RecipeManagement.FunctionalTests.TestUtilities;
using System.Net;
using System.Threading.Tasks;

public class UpdateRecipeRecordTests : TestBase
{
    [Fact]
    public async Task put_recipe_returns_nocontent_when_entity_exists()
    {
        // Arrange
        var recipe = new FakeRecipeBuilder().Build();
        var updatedRecipeDto = new FakeRecipeForUpdateDto().Generate();
        await InsertAsync(recipe);

        // Act
        var route = ApiRoutes.Recipes.Put(recipe.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedRecipeDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}