using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

public record CreateOrderRequest
{
    [JsonPropertyName("request")] public required CreateOrderRequestData Request { get; init; }
}