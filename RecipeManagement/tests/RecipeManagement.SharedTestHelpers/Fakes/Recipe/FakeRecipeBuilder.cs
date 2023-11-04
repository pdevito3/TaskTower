namespace RecipeManagement.SharedTestHelpers.Fakes.Recipe;

using RecipeManagement.Domain.Recipes;
using RecipeManagement.Domain.Recipes.Models;

public class FakeRecipeBuilder
{
    private RecipeForCreation _creationData = new FakeRecipeForCreation().Generate();

    public FakeRecipeBuilder WithModel(RecipeForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeRecipeBuilder WithTitle(string title)
    {
        _creationData.Title = title;
        return this;
    }
    
    public FakeRecipeBuilder WithDirections(string directions)
    {
        _creationData.Directions = directions;
        return this;
    }
    
    public FakeRecipeBuilder WithRecipeSourceLink(string recipeSourceLink)
    {
        _creationData.RecipeSourceLink = recipeSourceLink;
        return this;
    }
    
    public FakeRecipeBuilder WithDescription(string description)
    {
        _creationData.Description = description;
        return this;
    }
    
    public FakeRecipeBuilder WithImageLink(string imageLink)
    {
        _creationData.ImageLink = imageLink;
        return this;
    }
    
    public FakeRecipeBuilder WithDateOfOrigin(DateOnly? dateOfOrigin)
    {
        _creationData.DateOfOrigin = dateOfOrigin;
        return this;
    }
    
    public Recipe Build()
    {
        var result = Recipe.Create(_creationData);
        return result;
    }
}