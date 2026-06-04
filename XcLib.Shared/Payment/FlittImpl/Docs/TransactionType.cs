using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    [JsonStringEnumMemberName("purchase")] Purchase,

    [JsonStringEnumMemberName("verification")]
    Verification,

    [JsonStringEnumMemberName("p2p credit")]
    P2PCredit,

    [JsonStringEnumMemberName("p2p transfer")]
    P2PTransfer,

    [JsonStringEnumMemberName("settlement")]
    Settlement,
    [JsonStringEnumMemberName("reverse")] Reverse
}