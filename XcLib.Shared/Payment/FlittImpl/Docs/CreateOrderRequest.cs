using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl;

public record CreateOrderRequest
{
    [JsonPropertyName("version")] public string Version { get; init; } = "1.0.1";

    [JsonPropertyName("order_id")] public required string OrderId { get; init; }

    [JsonPropertyName("merchant_id")] public required int MerchantId { get; init; }

    [JsonPropertyName("order_desc")] public required string OrderDescription { get; init; }

    [JsonPropertyName("amount")] public required int Amount { get; init; }

    [JsonPropertyName("currency")] public required Currency Currency { get; init; }

    [JsonPropertyName("response_url")] public string? ResponseUrl { get; init; }

    [JsonPropertyName("server_callback_url")]
    public string? ServerCallbackUrl { get; init; }

    [JsonPropertyName("signature")] public required string Signature { get; init; }

    [JsonPropertyName("lifetime")] public int? Lifetime { get; init; }

    [JsonPropertyName("merchant_data")] public string? MerchantData { get; init; }

    [JsonPropertyName("preauth")] public string? Preauth { get; init; }

    [JsonPropertyName("sender_email")] public string? SenderEmail { get; init; }

    [JsonPropertyName("delayed")] public string? Delayed { get; init; }

    [JsonPropertyName("lang")] public string? Language { get; init; }

    [JsonPropertyName("product_id")] public string? ProductId { get; init; }

    [JsonPropertyName("required_rectoken")]
    public string? RequiredRectoken { get; init; }

    [JsonPropertyName("verification")] public string? Verification { get; init; }

    [JsonPropertyName("rectoken")] public string? Rectoken { get; init; }

    [JsonPropertyName("receiver_rectoken")]
    public string? ReceiverRectoken { get; init; }

    [JsonPropertyName("design_id")] public string? DesignId { get; init; }

    [JsonPropertyName("subscription")] public string? Subscription { get; init; }

    [JsonPropertyName("subscription_callback_url")]
    public string? SubscriptionCallbackUrl { get; init; }

    [JsonPropertyName("chargeback_callback_url")]
    public string? ChargebackCallbackUrl { get; init; }

    [JsonPropertyName("cancel_url")] public string? CancelUrl { get; init; }

    [JsonPropertyName("reservation_data")] public string? ReservationData { get; init; }
}