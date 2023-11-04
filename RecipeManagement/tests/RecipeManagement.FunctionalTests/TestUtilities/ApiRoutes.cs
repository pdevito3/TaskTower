namespace RecipeManagement.FunctionalTests.TestUtilities;
public class ApiRoutes
{
    public const string Base = "api";
    public const string Health = Base + "/health";

    // new api route marker - do not delete

    public static class Recipes
    {
        public static string GetList => $"{Base}/recipes";
        public static string GetAll => $"{Base}/recipes/all";
        public static string GetRecord(Guid id) => $"{Base}/recipes/{id}";
        public static string Delete(Guid id) => $"{Base}/recipes/{id}";
        public static string Put(Guid id) => $"{Base}/recipes/{id}";
        public static string Create => $"{Base}/recipes";
        public static string CreateBatch => $"{Base}/recipes/batch";
    }
}
