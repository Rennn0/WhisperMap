namespace XatiCraft.Data.Repos.MongoImpl.Model;

/// <summary>
/// </summary>
/// <param name="OriginalFile"></param>
/// <param name="FileKey"></param>
/// <param name="Location"></param>
/// <param name="ProductId"></param>
internal record ProductMetadata(
    string OriginalFile,
    string FileKey,
    string Location,
    long ProductId) : MongoDoc;