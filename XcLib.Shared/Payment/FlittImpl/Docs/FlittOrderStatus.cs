using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlittOrderStatus
{
    [JsonStringEnumMemberName("created")] Created,

    [JsonStringEnumMemberName("processing")]
    Processing,
    [JsonStringEnumMemberName("declined")] Declined,
    [JsonStringEnumMemberName("approved")] Approved,
    [JsonStringEnumMemberName("expired")] Expired,
    [JsonStringEnumMemberName("reversed")] Reversed
}