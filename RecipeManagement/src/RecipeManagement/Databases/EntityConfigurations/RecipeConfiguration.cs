namespace RecipeManagement.Databases.EntityConfigurations;

using RecipeManagement.Domain.Recipes;
using RecipeManagement.Domain.Visibilities;
using RecipeManagement.Domain.Ratings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    /// <summary>
    /// The database configuration for Recipes. 
    /// </summary>
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        // Relationship Marker -- Deleting or modifying this comment could cause incomplete relationship scaffolding

        // Property Marker -- Deleting or modifying this comment could cause incomplete relationship scaffolding

        builder.Property(x => x.Visibility)
            .HasConversion(x => x.Value, x => new Visibility(x));

        builder.Property(x => x.Rating)
            .HasConversion(x => x.Value, x => new Rating(x));
        
        // example for a more complex value object
        // builder.OwnsOne(x => x.PhysicalAddress, opts =>
        // {
        //     opts.Property(x => x.Line1).HasColumnName("physical_address_line1");
        //     opts.Property(x => x.Line2).HasColumnName("physical_address_line2");
        //     opts.Property(x => x.City).HasColumnName("physical_address_city");
        //     opts.Property(x => x.State).HasColumnName("physical_address_state");
        //     opts.OwnsOne(x => x.PostalCode, o =>
        //         {
        //             o.Property(x => x.Value).HasColumnName("physical_address_postal_code");
        //         }).Navigation(x => x.PostalCode)
        //         .IsRequired();
        //     opts.Property(x => x.Country).HasColumnName("physical_address_country");
        // }).Navigation(x => x.PhysicalAddress)
        //     .IsRequired();
    }
}
