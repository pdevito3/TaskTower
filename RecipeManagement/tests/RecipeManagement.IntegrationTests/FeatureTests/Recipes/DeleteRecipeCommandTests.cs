namespace RecipeManagement.IntegrationTests.FeatureTests.Recipes;

using RecipeManagement.SharedTestHelpers.Fakes.Recipe;
using RecipeManagement.Domain.Recipes.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteRecipeCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_recipe_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var recipe = new FakeRecipeBuilder().Build();
        await testingServiceScope.InsertAsync(recipe);

        // Act
        var command = new DeleteRecipe.Command(recipe.Id);
        await testingServiceScope.SendAsync(command);
        var recipeResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Recipes
                .CountAsync(r => r.Id == recipe.Id));

        // Assert
        recipeResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_recipe_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteRecipe.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_recipe_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var recipe = new FakeRecipeBuilder().Build();
        await testingServiceScope.InsertAsync(recipe);

        // Act
        var command = new DeleteRecipe.Command(recipe.Id);
        await testingServiceScope.SendAsync(command);
        var deletedRecipe = await testingServiceScope.ExecuteDbContextAsync(db => db.Recipes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == recipe.Id));

        // Assert
        deletedRecipe?.IsDeleted.Should().BeTrue();
    }
}