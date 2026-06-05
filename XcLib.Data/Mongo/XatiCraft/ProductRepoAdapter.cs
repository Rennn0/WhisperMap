using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

/// <inheritdoc cref="IProductRepo" />
public class ProductRepoAdapter : MongoBase<Product>, IProductRepo
{
    public ProductRepoAdapter(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }


    public async Task<ApplicationObjects.Product> GetByIdAsync(string objId, CancellationToken cancellationToken)
    {
        Product pDoc = await Collection.Find(p => p.Id == ObjectId.Parse(objId), new FindOptions { BatchSize = 1 })
            .SingleAsync(cancellationToken);
        return new ApplicationObjects.Product(pDoc.Title, pDoc.Description, pDoc.Price) { ObjId = pDoc.Id.ToString() };
    }

    public async Task<ApplicationObjects.Product>
        AddAsync(ApplicationObjects.Product obj, CancellationToken token = default) 

    {
        Product pDoc = new Product(obj.Title, obj.Description, obj.Price);
        await Collection.InsertOneAsync(pDoc,
            new InsertOneOptions { BypassDocumentValidation = true }, token);
        return obj with { ObjId = pDoc.Id.ToString() };
    }

    public Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<ApplicationObjects.Product> GetByIdAsync(long id, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<List<ApplicationObjects.Product>> GetAsync(ApplicationObjects.Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<ApplicationObjects.Product?> UpdateAsync(ApplicationObjects.Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<int> DeleteAsync(ApplicationObjects.Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<bool> ExistsAsync(ApplicationObjects.Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<List<ApplicationObjects.Product>> GetAsync(IEnumerable<long>? ids = null, OrderBy? orderBy = null,
        string? query = null, SearchCursor? cursor = null,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}