namespace RecipeManagement.Domain.Visibilities;

using Ardalis.SmartEnum;
using RecipeManagement.Exceptions;

public sealed class Visibility : ValueObject
{
    private VisibilityEnum _visibility;
    public string Value
    {
        get => _visibility.Name;
        private set
        {
            if (!VisibilityEnum.TryFromName(value, true, out var parsed))
                throw new InvalidSmartEnumPropertyName(nameof(Value), value);

            _visibility = parsed;
        }
    }
    
    public Visibility(string value)
    {
        Value = value;
    }

    public static Visibility Of(string value) => new Visibility(value);
    public static implicit operator string(Visibility value) => value.Value;
    public static List<string> ListNames() => VisibilityEnum.List.Select(x => x.Name).ToList();

   public static Visibility Public() => new Visibility(VisibilityEnum.Public.Name);
   public static Visibility FriendsOnly() => new Visibility(VisibilityEnum.FriendsOnly.Name);
   public static Visibility Private() => new Visibility(VisibilityEnum.Private.Name);

    private Visibility() { } // EF Core

    private abstract class VisibilityEnum : SmartEnum<VisibilityEnum>
    {
      public static readonly VisibilityEnum Public = new PublicType();
      public static readonly VisibilityEnum FriendsOnly = new FriendsOnlyType();
      public static readonly VisibilityEnum Private = new PrivateType();

       protected VisibilityEnum(string name, int value) : base(name, value)
       {
       }

       private class PublicType : VisibilityEnum
        {
            public PublicType() : base("Public", 0)
            {
            }
        }

       private class FriendsOnlyType : VisibilityEnum
        {
            public FriendsOnlyType() : base("Friends Only", 1)
            {
            }
        }

       private class PrivateType : VisibilityEnum
        {
            public PrivateType() : base("Private", 2)
            {
            }
        }
    }
}