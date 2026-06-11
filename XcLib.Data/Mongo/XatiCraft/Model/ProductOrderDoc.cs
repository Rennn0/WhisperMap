using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace XcLib.Data.Mongo.XatiCraft.Model;

[Description("product_order")]
public record ProductOrderDoc : MongoDoc
{
    [BsonElement("amount")] public long Amount { get; init; }
    [BsonElement("product_id")] public long ProductId { get; init; }
    [BsonElement("owner")] public required string OrderOwner { get; init; }
    [BsonElement("provider")] public sbyte PaymentProvider { get; init; }
    [BsonElement("status")] public string? OrderStatus { get; init; }
    [BsonElement("checkout")] public string? CheckoutUrl { get; init; }
    [BsonElement("p_ord_id")] public string? ProviderOrderId { get; init; }
    [BsonElement("ord_id")] public string? InternalOrderId { get; init; }
}