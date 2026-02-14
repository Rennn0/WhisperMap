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

    /// <summary>
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// </summary>
    public string? Picture { get; set; }

    /// <summary>
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// </summary>
    public string? Uid { get; set; }
}