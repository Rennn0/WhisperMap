using System.ComponentModel;

namespace XcLib.Data.Mongo.XatiCraft.Model;

[Description("product_cart")]
public record ProductCartDoc : MongoDoc
{
    public string? UserId { get; init; }
    public HashSet<string> ProductIds { get; init; } = [];
}