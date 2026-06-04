using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

public record GetOrderStatusResponse
{
    [JsonPropertyName("response")] public required GetOrderStatusResponseData Response { get; init; }
}