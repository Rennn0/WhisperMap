using Microsoft.Extensions.Options;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

public class ProductMetadataRepo : MongoBase<ProductMetadataDoc>, IProductMetadaRepo
{
    public ProductMetadataRepo(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public Task<ProductMetadata> GetByIdAsync(object id, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<ProductMetadata> AddAsync(ProductMetadata obj, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<List<ProductMetadata>> GetAsync(ProductMetadata obj,
        sbyte searchFlag = 0, CancellationToken token = default) => throw new NotImplementedException();

    public Task<ProductMetadata?> UpdateAsync(ProductMetadata obj,
        sbyte searchFlag = 0, CancellationToken token = default) => throw new NotImplementedException();

    public Task<int> DeleteAsync(ProductMetadata obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<bool> ExistsAsync(ProductMetadata obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();
}