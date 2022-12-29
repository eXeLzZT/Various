using System.Text.RegularExpressions;

namespace Various.Utils;

public static class StringExtensions
{
    public static Match Match(this string text, int position)
    {
        return new Regex(Constants.UrlPattern).Match(text, position);
    }
}
