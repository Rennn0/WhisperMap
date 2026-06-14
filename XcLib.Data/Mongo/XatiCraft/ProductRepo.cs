using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

/// <inheritdoc cref="IProductRepo" />
public class ProductRepo : MongoBase<ProductDoc>, IProductRepo
{
    public ProductRepo(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }


    public async Task<Product> GetByIdAsync(string objId, CancellationToken cancellationToken)
    {
        ProductDoc pDoc = await Collection.Find(p => p.Id == ObjectId.Parse(objId), new FindOptions { BatchSize = 1 })
            .SingleAsync(cancellationToken);
        return new Product(pDoc.Title, pDoc.Description, pDoc.Price) { ObjId = pDoc.Id.ToString() };
    }

    public async Task<Product>
        AddAsync(Product obj, CancellationToken token = default) 

    {
        ProductDoc pDoc = new ProductDoc(obj.Title, obj.Description, obj.Price);
        await Collection.InsertOneAsync(pDoc,
            new InsertOneOptions { BypassDocumentValidation = true }, token);
        return obj with { ObjId = pDoc.Id.ToString() };
    }

    public Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<Product> GetByIdAsync(object id, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<List<Product>> GetAsync(Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<Product?> UpdateAsync(Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<int> DeleteAsync(Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<bool> ExistsAsync(Product obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<List<Product>> GetAsync(IEnumerable<long>? ids = null, OrderBy? orderBy = null,
        string? query = null, SearchCursor? cursor = null,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}