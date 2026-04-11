using System.Text.Json.Serialization;

namespace XcLib.Data.Abstractions;

public abstract record SearchCursor
{
    private const uint MaxBatchSize = 50;
    private const uint DefaultBatchSize = 5;
    private uint? _batchSize;
    [JsonPropertyName("0")] public long? Id { get; init; }

    [JsonPropertyName("1")]
    public uint BatchSize
    {
        get => _batchSize ?? DefaultBatchSize;
        set => _batchSize = Math.Min(value, MaxBatchSize);
    }

    [JsonPropertyName("2")] public DateTime? Timestamp { get; init; }
    [JsonPropertyName("3")] public decimal? Price { get; init; }

    public abstract string Encode(SearchCursor cursor);

    public abstract SearchCursor? Decode(string? token);
}