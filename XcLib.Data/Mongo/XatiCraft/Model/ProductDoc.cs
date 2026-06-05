using System.ComponentModel;

namespace XcLib.Data.Mongo.XatiCraft.Model;

[Description("product")]
public record ProductDoc(string Title, string Description, decimal Price) : MongoDoc
{
    /// <summary>
    /// </summary>
    public ICollection<ProductMetadataDoc>? ProductMetadata { get; init; }
}