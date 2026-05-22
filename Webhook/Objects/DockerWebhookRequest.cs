using System.Text.Json.Serialization;

namespace Webhook.Objects;

internal record DockerWebhookRequest
{
    [JsonPropertyName("push_data")] public PushData? Push { get; init; }
    [JsonIgnore] public sbyte? Index { get; set; }
    [JsonIgnore] public sbyte? ExecutionState { get; init; }
    [JsonIgnore] public string? StdOut { get; set; }
    [JsonIgnore] public string? StdErr { get; set; }
    [JsonIgnore] public string? Service { get; init; }
    [JsonIgnore] public string? Img { get; init; }
}