namespace RecipeManagement.Domain.Recipes.DomainEvents;

public sealed class RecipeCreated : DomainEvent
{
    public Recipe Recipe { get; set; } 
}
            