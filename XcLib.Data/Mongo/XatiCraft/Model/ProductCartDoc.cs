using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace XcLib.Data.Mongo.XatiCraft.Model;

[Description("product_cart")]
public record ProductCartDoc : MongoDoc
{
    public string? UserId { get; init; }
    public HashSet<string>? ProductIds { get; init; }
    [BsonElement("p_orders")] public HashSet<string>? ProductOrderIds  { get; init; } 
}