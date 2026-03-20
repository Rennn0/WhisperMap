namespace XatiCraft.Data.Repos.MongoImpl.Model;

internal record ProductCart : MongoDoc
{
    public string? UserId { get; init; }
    public HashSet<string>? ProductIds { get; init; }
}