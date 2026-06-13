using System.Text.Json.Serialization;
using MongoDB.Bson;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.ApplicationObjects;

public record ProductOrder(
    [property: JsonIgnore] string OrderOwner,
    [property: JsonIgnore] long ProductId,
    [property: JsonIgnore] sbyte PaymentProvider,
    [property: JsonIgnore] long Amount)
    : ApplicationObject
{
    public ProductOrder() : this(null!, 0, 0, 0)
    {
    }

    public string? OrderStatus { get; set; }
    public string? CheckoutUrl { get; init; }
    public string? ProviderOrderId { get; init; }
    public string? InternalOrderId { get; init; }

    public static ProductOrder From(ProductOrderDoc doc) =>
        new ProductOrder(doc.OrderOwner, doc.ProductId, doc.PaymentProvider, doc.Amount)
        {
            ObjId = doc.Id.ToString(),
            CheckoutUrl = doc.CheckoutUrl,
            OrderStatus = doc.OrderStatus,
            ProviderOrderId = doc.ProviderOrderId,
            InternalOrderId = doc.InternalOrderId
        };

    public static ProductOrderDoc From(ProductOrder obj) => new ProductOrderDoc
    {
        OrderOwner = obj.OrderOwner,
        CheckoutUrl = obj.CheckoutUrl,
        OrderStatus = obj.OrderStatus,
        PaymentProvider = obj.PaymentProvider,
        ProductId = obj.ProductId,
        Amount = obj.Amount,
        ProviderOrderId = obj.ProviderOrderId,
        InternalOrderId = obj.InternalOrderId,
        Id = ObjectId.TryParse(obj.ObjId, out ObjectId objectId) ? objectId : ObjectId.Empty
    };
}