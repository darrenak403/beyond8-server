using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beyond8.Learning.Application.Dtos.Catalog;

public sealed class CourseStatusIntConverter : JsonConverter<int>
{
    private static readonly Dictionary<string, int> NameToValue = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Draft"] = 0,
        ["PendingApproval"] = 1,
        ["Approved"] = 2,
        ["Rejected"] = 3,
        ["Published"] = 4,
        ["Archived"] = 5,
        ["Suspended"] = 6
    };

    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => ParseString(reader.GetString()),
            _ => throw new JsonException($"Unexpected token {reader.TokenType} for Status.")
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }

    private static int ParseString(string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new JsonException("Status string cannot be null or empty.");
        if (NameToValue.TryGetValue(value, out var num))
            return num;
        throw new JsonException($"Unknown course status: '{value}'.");
    }
}
