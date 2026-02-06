using XatiCraft.Data.Repos.MongoImpl.Model;

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

    /// <inheritdoc />
    public Task<Objects.Product> InsertAsync(Objects.Product product, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}