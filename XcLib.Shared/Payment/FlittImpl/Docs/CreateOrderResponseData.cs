using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

public record CreateOrderResponseData
{
    [JsonPropertyName("response_status")] public required ResponseStatus ResponseStatus { get; init; }

    [JsonPropertyName("checkout_url")] public string? CheckoutUrl { get; init; }

    [JsonPropertyName("error_message")] public string? ErrorMessage { get; init; }

    [JsonPropertyName("error_code")] public object? ErrorCode { get; init; }

    [JsonIgnore] public bool IsSuccess => ResponseStatus == ResponseStatus.Success;
}