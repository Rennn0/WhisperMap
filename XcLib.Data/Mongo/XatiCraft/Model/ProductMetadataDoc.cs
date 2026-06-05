using System.ComponentModel;

namespace XcLib.Data.Mongo.XatiCraft.Model;

[Description("product_meta")]
public record ProductMetadataDoc(
    string OriginalFile,
    string FileKey,
    string Location,
    long ProductId) : MongoDoc;