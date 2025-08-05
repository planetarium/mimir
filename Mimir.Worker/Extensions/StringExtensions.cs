using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
public static class StringExtensions
{
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var words = str.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        var pascalCaseWords = words.Select(word =>
            char.ToUpper(word[0]) + word.Substring(1).ToLower()
        );

        return string.Concat(pascalCaseWords);
    }
}
