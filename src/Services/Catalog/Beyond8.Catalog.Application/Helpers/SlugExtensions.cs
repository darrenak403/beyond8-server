using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Beyond8.Catalog.Application.Helpers
{
    public static class SlugExtensions
    {
        public static string ToSlug(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // 1. Chuyển sang chữ thường
            input = input.ToLowerInvariant();

            // 2. Thay thế các từ đặc biệt
            foreach (var map in SpecialMappings)
            {
                input = Regex.Replace(
                    input,
                    Regex.Escape(map.Key),
                    map.Value,
                    RegexOptions.IgnoreCase
                );
            }

            // 3. Thay thế các ký tự đặc biệt
            input = input.Replace("#", " sharp ");
            input = input.Replace("+", " plus ");
            input = input.Replace("&", " and ");
            input = input.Replace("@", " at ");
            input = input.Replace("%", " percent ");
            input = input.Replace("đ", "d");


            // 4. Chuẩn hóa chuỗi sang Unicode Form D (Tách dấu ra khỏi chữ cái)
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            // 5. Lặp qua từng ký tự, chỉ giữ lại ký tự không phải là dấu (NonSpacingMark)
            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // 6. Chuyển về dạng string bình thường (Form C)
            var cleanString = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // 7. Dùng Regex để loại bỏ ký tự đặc biệt, giữ lại chữ cái, số và khoảng trắng
            cleanString = Regex.Replace(cleanString, @"[^a-z0-9\s-]", "");

            // 8. Thay thế khoảng trắng bằng dấu gạch ngang
            cleanString = Regex.Replace(cleanString, @"\s+", "-");

            // 9. Xóa các dấu gạch ngang liên tiếp (ví dụ: a---b -> a-b)
            cleanString = Regex.Replace(cleanString, @"-+", "-");

            // 10. Cắt bỏ gạch ngang ở đầu và cuối chuỗi
            return cleanString.Trim('-');
        }

        private static readonly Dictionary<string, string> SpecialMappings = new()
        {
            { "c#", "c sharp" },
            { "c++", "c plus plus" },
            { ".net", "dotnet" },
            { "node.js", "nodejs" },
            { "asp.net", "aspnet" },
            { "f#", "f sharp" },
            { "ai", "ai" },
            { "ml", "ml" }
        };
    }
}