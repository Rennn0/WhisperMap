namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
public record ApiContract
{
    /// <summary>
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString("N");
}