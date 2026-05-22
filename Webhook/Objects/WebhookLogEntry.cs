namespace Webhook.Objects;

internal readonly struct WebhookLogEntry
{
    public string? Image { get; init; }
    public string? Time { get; init; }
}