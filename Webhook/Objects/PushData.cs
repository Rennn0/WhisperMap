using System.Text.Json.Serialization;

namespace Webhook.Objects;

internal readonly struct PushData
{
    [JsonPropertyName("tag")] public string? Tag { get; init; }
}