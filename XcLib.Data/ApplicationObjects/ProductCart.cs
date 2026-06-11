using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.ApplicationObjects;

/// <inheritdoc />
public record ProductCart(string UserId) : ApplicationObject
{
    public HashSet<string>? ProductIds { get; init; }
    public HashSet<string>? ProductOrderIds { get; init; }

    public static ProductCart From(ProductCartDoc doc) => new ProductCart(doc.UserId!)
    {
        ProductOrderIds =  doc.ProductOrderIds,
        ProductIds = doc.ProductIds,
        ObjId = doc.Id.ToString()
    };
}