namespace XatiCraft.Data.Objects;

/// <inheritdoc />
public record ProductCart(string UserId) : ApplicationObject
{
    /// <summary>
    /// </summary>
    public HashSet<string>? ProductIds { get; init; }
}