using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment;

public abstract record RootObj
{
    [JsonPropertyName("k")] public bool? Ok { get; init; }
    [JsonPropertyName("e")] public string? Error { get; init; }
}