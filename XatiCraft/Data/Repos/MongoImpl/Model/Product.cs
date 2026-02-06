namespace XatiCraft.Data.Repos.MongoImpl.Model;

/// <summary>
/// </summary>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="Price"></param>
internal record Product(string Title, string Description, decimal Price) : MongoDoc
{
    /// <summary>
    /// </summary>
    public ICollection<ProductMetadata>? ProductMetadata { get; init; }
}