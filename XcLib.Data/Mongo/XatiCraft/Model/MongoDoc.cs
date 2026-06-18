using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace XcLib.Data.Mongo.XatiCraft.Model;

/// <summary>
/// </summary>
public abstract record MongoDoc
{
    /// <summary>
    /// </summary>
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; init; }
}