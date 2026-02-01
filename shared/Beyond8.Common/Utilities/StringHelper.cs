using System.Globalization;
using System.Text;

namespace Beyond8.Common.Utilities;

/// <summary>
/// Helper class for string manipulation, including Vietnamese diacritics removal
/// and PostgreSQL full-text search term formatting.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Removes diacritics (accent marks) from Vietnamese text.
    /// Example: "Lập trình" -> "Lap trinh"
    /// </summary>
    /// <param name="text">Input text with diacritics</param>
    /// <returns>Text without diacritics, lowercase and trimmed</returns>
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

    /// <summary>
    /// Formats search term for PostgreSQL full-text search.
    /// Removes diacritics and joins words with " & " for AND matching.
    /// Example: "lập trình web" -> "lap & trinh & web"
    /// </summary>
    /// <param name="input">Raw search input from user</param>
    /// <returns>Formatted search term for PostgreSQL tsquery</returns>
    public static string FormatSearchTerm(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var unsign = RemoveDiacritics(input);

        // Split by whitespace, remove empty entries, join with & operator
        var terms = unsign.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" & ", terms);
    }

    /// <summary>
    /// Formats search term for PostgreSQL full-text search with prefix matching.
    /// Example: "lap trinh" -> "lap:* & trinh:*"
    /// This allows partial word matching.
    /// </summary>
    /// <param name="input">Raw search input from user</param>
    /// <returns>Formatted search term with prefix matching</returns>
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
