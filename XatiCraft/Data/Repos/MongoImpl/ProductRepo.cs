using MongoDB.Driver;
using Product = XatiCraft.Data.Repos.MongoImpl.Model.Product;

namespace XatiCraft.Data.Repos.MongoImpl;

/// <inheritdoc cref="IProductRepo" />
internal class ProductRepo : MongoBase<Product>, IProductRepo
{
    public ProductRepo(string connection, string db = "xc-db") : base(connection, db)
    {
    }

    /// <inheritdoc />
    public Task<List<Objects.Product>> SelectProductsAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Objects.Product?> SelectProductAsync(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Objects.Product?> SelectProductAsync(string objId, CancellationToken cancellationToken)
    {
        Product? pDoc = await Collection.Find(p => p.Id == objId, new FindOptions { BatchSize = 1 })
            .FirstOrDefaultAsync(cancellationToken);
        return new Objects.Product(pDoc.Title, pDoc.Description, pDoc.Price) { ObjId = pDoc.Id };
    }

    /// <inheritdoc />
    public async Task<Objects.Product> InsertAsync(Objects.Product product, CancellationToken cancellationToken)
    {
        Product pDoc = new(product.Title, product.Description, product.Price);
        await Collection.InsertOneAsync(pDoc,
            new InsertOneOptions { BypassDocumentValidation = false }, cancellationToken);
        product.ObjId = pDoc.Id;
        return product;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}