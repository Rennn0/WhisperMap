namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public record UserInfo
{
    /// <summary>
    /// </summary>
    public bool CanUpload { get; set; }

    /// <summary>
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// </summary>
    public HashSet<ApplicationClaims> Claims { get; } = [];
}