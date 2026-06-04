using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

public record GetOrderStatusRequest
{
    [JsonPropertyName("request")] public required GetOrderStatusRequestData Request { get; init; }
}