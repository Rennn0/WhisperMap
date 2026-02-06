using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace XatiCraft.Data.Repos.MongoImpl.Model;

/// <summary>
/// </summary>
internal record MongoDoc
{
    /// <summary>
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = ObjectId.GenerateNewId().ToString();
}