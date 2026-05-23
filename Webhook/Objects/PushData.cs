using System.Text.Json.Serialization;

namespace Webhook.Objects;

internal record PushData
{
    [JsonPropertyName("tag")] public string? Tag { get; init; }
}