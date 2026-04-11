using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

/// <inheritdoc cref="IProductRepo" />
public class ProductRepo : MongoBase<Product>, IProductRepo
{
    public ProductRepo(string connection, string db = "xc-db") : base(connection, db)
    {
    }

    /// <inheritdoc />
    public Task<List<ApplicationObjects.Product>> SelectAsync(IEnumerable<long>? ids = null, OrderBy? orderBy = null,
        string? query = null,
        SearchCursor? cursor = null,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<ApplicationObjects.Product?> SelectAsync(long id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public async Task<ApplicationObjects.Product?> SelectAsync(string objId, CancellationToken cancellationToken)
    {
        Product? pDoc = await Collection.Find(p => p.Id == objId, new FindOptions { BatchSize = 1 })
            .FirstOrDefaultAsync(cancellationToken);
        return pDoc is null
            ? null
            : new ApplicationObjects.Product(pDoc.Title, pDoc.Description, pDoc.Price) { ObjId = pDoc.Id };
    }

    /// <inheritdoc />
    public async Task<ApplicationObjects.Product> InsertAsync(ApplicationObjects.Product product,
        CancellationToken cancellationToken)
    {
        Product pDoc = new Product(product.Title, product.Description, product.Price);
        await Collection.InsertOneAsync(pDoc,
            new InsertOneOptions { BypassDocumentValidation = false }, cancellationToken);
        return product with { ObjId = pDoc.Id };
    }

    public Task<ApplicationObjects.Product> UpdateAsync(ApplicationObjects.Product product,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc />
    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken) => throw new NotImplementedException();
}