namespace RecipeManagement.Domain.Ratings;

using FluentValidation;

public sealed class Rating : ValueObject
{
    public int? Value { get; set; }
    
    public Rating(int? value)
    {
        Value = value;
    }
    
    public static Rating Of(int? value) => new Rating(value);
    public static implicit operator string(Rating value) => value.Value.ToString();

    private Rating() { } // EF Core
    
    private sealed class RatingValidator : AbstractValidator<int?> 
    {
        public RatingValidator()
        {
        }
    }
}