using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerificationStatus
{
    [JsonStringEnumMemberName("verified")] Verified,

    [JsonStringEnumMemberName("incorrect")]
    Incorrect,
    [JsonStringEnumMemberName("failed")] Failed,
    [JsonStringEnumMemberName("created")] Created
}