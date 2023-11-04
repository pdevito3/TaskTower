namespace RecipeManagement.Services;

using RecipeManagement.Databases;

public interface IUnitOfWork : IRecipeManagementScopedService
{
    Task<int> CommitChanges(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly RecipesDbContext _dbContext;

    public UnitOfWork(RecipesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CommitChanges(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
