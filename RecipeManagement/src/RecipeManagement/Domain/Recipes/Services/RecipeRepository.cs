namespace RecipeManagement.Domain.Recipes.Services;

using RecipeManagement.Domain.Recipes;
using RecipeManagement.Databases;
using RecipeManagement.Services;

public interface IRecipeRepository : IGenericRepository<Recipe>
{
}

public sealed class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
{
    private readonly RecipesDbContext _dbContext;

    public RecipeRepository(RecipesDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
