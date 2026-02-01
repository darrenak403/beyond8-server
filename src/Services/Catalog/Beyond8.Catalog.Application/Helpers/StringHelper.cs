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

        // Split by whitespace, remove empty entries, join with & operator
        var terms = unsign.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" & ", terms);
    }

    public static string FormatSearchTermWithPrefix(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var unsign = RemoveDiacritics(input);

        // Split by whitespace, add :* suffix for prefix matching, join with & operator
        var terms = unsign.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" & ", terms.Select(t => $"{t}:*"));
    }
}
