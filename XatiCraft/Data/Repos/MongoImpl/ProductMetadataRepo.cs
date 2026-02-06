using XatiCraft.Data.Repos.MongoImpl.Model;

namespace XatiCraft.Data.Repos.MongoImpl;

/// <inheritdoc cref="IProductMetadaRepo" />
internal class ProductMetadataRepo : MongoBase<ProductMetadata>, IProductMetadaRepo
{
    public ProductMetadataRepo(string connection, string db = "xc-db") : base(connection, db)
    {
    }

    /// <inheritdoc />
    public Task<Objects.ProductMetadata> InsertAsync(Objects.ProductMetadata productMetadata,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}