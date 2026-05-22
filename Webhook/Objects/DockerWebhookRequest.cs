using System.Text.Json.Serialization;

namespace Webhook.Objects;

internal struct DockerWebhookRequest
{
    [JsonPropertyName("push_data")] public PushData? Push { get; init; }
    [JsonIgnore]public sbyte? Index { get; set; }

    public override string ToString() => $"index {Index}, tag {Push?.Tag}";
}