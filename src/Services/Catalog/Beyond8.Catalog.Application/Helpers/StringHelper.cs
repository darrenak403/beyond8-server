using System.Globalization;
using System.Text;

namespace Beyond8.Catalog.Application.Helpers;

public static class StringHelper
{
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // 1. Normalize unicode to FormD (decomposed form)
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        // 2. Filter out diacritical marks (NonSpacingMark category)
        foreach (char c in normalizedString)
        {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // 3. Normalize back to FormC and convert to lowercase
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower().Trim();
    }

    public static string FormatSearchTerm(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var unsign = RemoveDiacritics(input);

        var sanitized = SanitizeForTsQuery(unsign);

        var terms = sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" & ", terms);
    }


    public static string FormatSearchTermPlain(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var unsign = RemoveDiacritics(input);
        var sanitized = SanitizeForTsQuery(unsign);
        var terms = sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", terms);
    }

    public static string SanitizeForTsQuery(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c is '&' or '|' or '!' or ':' or '*' or '(' or ')' or '\\')
                sb.Append(' ');
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    public static string FormatSearchTermWithPrefix(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var unsign = RemoveDiacritics(input);
        var sanitized = SanitizeForTsQuery(unsign);

        // Split by whitespace, add :* suffix for prefix matching, join with & operator
        var terms = sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" & ", terms.Select(t => $"{t}:*"));
    }
}
