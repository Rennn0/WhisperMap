using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResponseStatus
{
    [JsonStringEnumMemberName("success")] Success,

    [JsonStringEnumMemberName("failure")] Failure
}