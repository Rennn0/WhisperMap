using Microsoft.Extensions.Options;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

/// <inheritdoc cref="IProductMetadaRepo" />
public class ProductMetadataRepoAdapter : MongoBase<ProductMetadata>, IProductMetadaRepo
{
    public ProductMetadataRepoAdapter(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public Task<ApplicationObjects.ProductMetadata> GetByIdAsync(long id, CancellationToken token = default) => throw new NotImplementedException();

    public Task<ApplicationObjects.ProductMetadata> AddAsync(ApplicationObjects.ProductMetadata obj, CancellationToken token = default) => throw new NotImplementedException();

    public Task<List<ApplicationObjects.ProductMetadata>> GetAsync(ApplicationObjects.ProductMetadata obj,
        sbyte searchFlag = 0, CancellationToken token = default) => throw new NotImplementedException();

    public Task<ApplicationObjects.ProductMetadata?> UpdateAsync(ApplicationObjects.ProductMetadata obj,
        sbyte searchFlag = 0, CancellationToken token = default) => throw new NotImplementedException();

    public Task<int> DeleteAsync(ApplicationObjects.ProductMetadata obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<bool> ExistsAsync(ApplicationObjects.ProductMetadata obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();
}