namespace XcLib.Data.Mongo.XatiCraft.Model;

/// <summary>
/// </summary>
/// <param name="OriginalFile"></param>
/// <param name="FileKey"></param>
/// <param name="Location"></param>
/// <param name="ProductId"></param>
public record ProductMetadata(
    string OriginalFile,
    string FileKey,
    string Location,
    long ProductId) : MongoDoc;