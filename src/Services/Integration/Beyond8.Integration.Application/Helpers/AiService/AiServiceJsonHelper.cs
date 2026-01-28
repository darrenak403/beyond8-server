using System.Text.Json;
using System.Text.RegularExpressions;

namespace Beyond8.Integration.Application.Helpers.AiService
{
    public static class AiServiceJsonHelper
    {
        public static string? ExtractJson(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            var s = content.Trim();

            var match = Regex.Match(s, @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            var start = s.IndexOf('{');
            if (start < 0) return null;

            var inString = false;
            var escape = false;
            var depth = 1;
            for (var j = start + 1; j < s.Length; j++)
            {
                var c = s[j];
                if (escape) { escape = false; continue; }
                if (inString)
                {
                    if (c == '\\') { escape = true; continue; }
                    if (c == '"') { inString = false; continue; }
                    continue;
                }
                if (c == '"') { inString = true; continue; }
                if (c == '{') { depth++; continue; }
                if (c == '}')
                {
                    depth--;
                    if (depth == 0) return s.Substring(start, j - start + 1);
                }
            }
            return null;
        }

        public static JsonElement? GetPropertyIgnoreCase(JsonElement root, string name)
        {
            foreach (var p in root.EnumerateObject())
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    return p.Value;
            return null;
        }
    }
}
