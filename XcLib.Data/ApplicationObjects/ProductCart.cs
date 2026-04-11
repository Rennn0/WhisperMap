namespace XcLib.Data.ApplicationObjects;

/// <inheritdoc />
public record ProductCart(string UserId) : ApplicationObject
{
    /// <summary>
    /// </summary>
    public HashSet<string> ProductIds { get; init; } = [];
}