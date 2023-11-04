namespace RecipeManagement.Domain.Recipes.DomainEvents;

public sealed class RecipeUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            