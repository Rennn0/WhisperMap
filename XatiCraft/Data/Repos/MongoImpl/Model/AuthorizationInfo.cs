namespace XatiCraft.Data.Repos.MongoImpl.Model;

internal record AuthorizationInfo : MongoDoc
{
    public bool? AccountEnabled { get; init; }
    public bool? VerifiedEmail { get; init; }
    public string? Username { get; init; }
    public string? CreationToken { get; init; }
    public string? AuthProvider { get; init; }
    public string? ProfilePicture { get; init; }
    public string? Email { get; init; }
    public DateTimeOffset? Created { get; init; }
    public string? AuthProviderSystemId { get; init; }
    public string? AuthProviderOfficial { get; init; }
}