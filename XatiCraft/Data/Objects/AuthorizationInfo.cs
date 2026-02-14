namespace XatiCraft.Data.Objects;

/// <inheritdoc />
public record AuthorizationInfo(string Username, DateTimeOffset Created) : ApplicationObject
{
    /// <summary>
    /// </summary>
    public string? CreationToken { get; init; }

    /// <summary>
    /// </summary>
    public string? AuthProvider { get; init; }

    /// <summary>
    /// </summary>
    public string? AuthProviderOfficial { get; init; }

    /// <summary>
    /// </summary>
    public string? AuthProviderSystemId { get; init; }

    /// <summary>
    /// </summary>
    public string? ProfilePicture { get; init; }

    /// <summary>
    /// </summary>
    public string? Email { get; init; }


    /// <summary>
    /// </summary>
    public bool? AccountEnabled { get; init; }

    /// <summary>
    /// </summary>
    public bool? VerifiedEmail { get; init; }
}