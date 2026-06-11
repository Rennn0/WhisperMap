using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace XcLib.Data.Mongo.XatiCraft.Model;

[Description("payment_provider")]
public record PaymentProviderDoc(
    [property: BsonElement("name")] string Name,
    [property: BsonElement("uniq_selector")]
    sbyte UniqSelector) : MongoDoc
{
    [BsonElement("logo_url")]
    public string? LogoUrl { get; init; }

    [BsonElement("full_name")]
    public string? FullName { get; init; }

    [BsonElement("desc")]
    public string? Description { get; init; }

    [BsonElement("enabled")]
    public bool? Enabled { get; init; }
}