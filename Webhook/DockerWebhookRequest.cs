using System.Text.Json.Serialization;

namespace Webhook;

internal readonly struct DockerWebhookRequest
{
    [JsonPropertyName("push_data")] public PushData? Push { get; init; }
}