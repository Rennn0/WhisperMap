using System.Text.Json.Serialization;

namespace Webhook.Objects;

internal record DockerWebhookRequest
{
    [JsonPropertyName("push_data")] public PushData? Push { get; init; }
    [JsonIgnore] public sbyte? Index { get; set; }
    [JsonIgnore] public ExecutionState? State { get; init; }
    [JsonIgnore] public string? StdOut { get; init; }
    [JsonIgnore] public string? StdErr { get; init; }
    [JsonIgnore] public string? Service { get; init; }
    [JsonIgnore] public string? Img { get; init; }
}