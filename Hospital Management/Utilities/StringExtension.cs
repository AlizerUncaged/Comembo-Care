using System.Globalization;

namespace Hospital_Management.Utilities;

public static class StringExtension
{
    public static string FirstCharToUpper(this string input) =>
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
}