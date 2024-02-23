namespace TaskTower.Utils;

using System.Text.RegularExpressions;

public static class StringUtility
{
    public static string StripNonAlphanum(string input)
    {
        var re = new Regex(@"[^a-zA-Z0-9_]");
        return re.Replace(input, "");
    }
}
