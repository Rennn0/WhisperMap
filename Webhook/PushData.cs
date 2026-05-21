using System.Text.Json.Serialization;

namespace Webhook;

internal readonly struct PushData
{
    [JsonPropertyName("tag")] public string? Tag { get; init; }
}