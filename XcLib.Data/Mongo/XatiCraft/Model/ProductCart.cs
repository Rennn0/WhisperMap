namespace XcLib.Data.Mongo.XatiCraft.Model;

public record ProductCart : MongoDoc
{
    public string? UserId { get; init; }
    public HashSet<string> ProductIds { get; init; } = [];
}