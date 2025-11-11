namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public record UserInfo
{
    /// <summary>
    /// </summary>
    public bool CanUpload { get; init; }

    /// <summary>
    /// </summary>
    public HashSet<uint>? Claims { get; init; }
}