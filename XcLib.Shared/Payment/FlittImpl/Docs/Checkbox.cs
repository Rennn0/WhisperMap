using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Checkbox
{
    [JsonStringEnumMemberName("Y")] Yes,
    [JsonStringEnumMemberName("N")] No
}