using System.Text.Json;
using System.Text.Json.Serialization;

namespace XcLib.Sse.Formatters;

public class DefaultEventFormatter<T> : SseEventFormatter<T>
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        IncludeFields = false,
        NumberHandling = JsonNumberHandling.Strict,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        WriteIndented = false
    };

    protected override string FormatData(T data) =>
        JsonSerializer.Serialize(data, _options);
}