using Microsoft.Extensions.Options;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

/// <inheritdoc cref="IProductMetadaRepo" />
public class ProductMetadataRepo : MongoBase<ProductMetadata>, IProductMetadaRepo
{
    public ProductMetadataRepo(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    /// <inheritdoc />
    public Task<ApplicationObjects.ProductMetadata> InsertAsync(ApplicationObjects.ProductMetadata productMetadata,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}