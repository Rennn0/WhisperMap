using XatiCraft.Objects;

namespace XatiCraft.Data.Repos;

/// <summary>
/// </summary>
public interface IProductRepo
{
    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Product>> SelectProductsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Product?> SelectProductAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Product> InsertAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken);
}