using XatiCraft.Objects;

namespace XatiCraft.Data.Repos;

/// <summary>
///     defines functionality needed to manipulate Product entity
/// </summary>
public interface IProductRepo
{
    /// <summary>
    ///     returns data from VProducts (not paginated)
    /// </summary>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<List<Product>> SelectProductsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     returns one row by id matching from VProducts
    /// </summary>
    /// <param name="id">id looking for</param>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<Product?> SelectProductAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    ///     adds new row in Products table.
    ///     product object then be populated
    /// </summary>
    /// <param name="product">transport object</param>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<Product> InsertAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    ///     returns true if exists one Product matching by id
    /// </summary>
    /// <param name="id">id looking for</param>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken);
}