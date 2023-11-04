namespace RecipeManagement.Domain.Recipes;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using RecipeManagement.Exceptions;
using RecipeManagement.Domain.Recipes.Models;
using RecipeManagement.Domain.Recipes.DomainEvents;
using RecipeManagement.Domain.Ratings;
using RecipeManagement.Domain.Visibilities;


public class Recipe : BaseEntity
{
    public string Title { get; private set; }

    public string Directions { get; private set; }

    public string RecipeSourceLink { get; private set; }

    public string Description { get; private set; }

    public string ImageLink { get; private set; }

   public Rating Rating { get; private set; }

   public Visibility Visibility { get; private set; }

    public DateOnly? DateOfOrigin { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Recipe Create(RecipeForCreation recipeForCreation)
    {
        var newRecipe = new Recipe();

        newRecipe.Title = recipeForCreation.Title;
        newRecipe.Directions = recipeForCreation.Directions;
        newRecipe.RecipeSourceLink = recipeForCreation.RecipeSourceLink;
        newRecipe.Description = recipeForCreation.Description;
        newRecipe.ImageLink = recipeForCreation.ImageLink;
        newRecipe.Rating = Rating.Of(recipeForCreation.Rating);
        newRecipe.Visibility = Visibility.Of(recipeForCreation.Visibility);
        newRecipe.DateOfOrigin = recipeForCreation.DateOfOrigin;

        newRecipe.QueueDomainEvent(new RecipeCreated(){ Recipe = newRecipe });
        
        return newRecipe;
    }

    public Recipe Update(RecipeForUpdate recipeForUpdate)
    {
        Title = recipeForUpdate.Title;
        Directions = recipeForUpdate.Directions;
        RecipeSourceLink = recipeForUpdate.RecipeSourceLink;
        Description = recipeForUpdate.Description;
        ImageLink = recipeForUpdate.ImageLink;
        Rating = Rating.Of(recipeForUpdate.Rating);
        Visibility = Visibility.Of(recipeForUpdate.Visibility);
        DateOfOrigin = recipeForUpdate.DateOfOrigin;

        QueueDomainEvent(new RecipeUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Recipe() { } // For EF + Mocking
}