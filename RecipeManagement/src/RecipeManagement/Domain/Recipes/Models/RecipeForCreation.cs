namespace RecipeManagement.Domain.Recipes.Models;

using Destructurama.Attributed;

public sealed class RecipeForCreation
{
    public string Title { get; set; }
    public string Directions { get; set; }
    public string RecipeSourceLink { get; set; }
    public string Description { get; set; }
    public string ImageLink { get; set; }
    public int? Rating { get; set; }
    public string Visibility { get; set; }
    public DateOnly? DateOfOrigin { get; set; }
}
