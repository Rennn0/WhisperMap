using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Handlers.Impl;

namespace XatiCraft.Data.Repos;

/// <summary>
///     defines functionality needed to manipulate Product entity
/// </summary>
internal interface IProductRepo
{
    /// <summary>
    ///     returns data from VProducts (not paginated)
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="orderBy"></param>
    /// <param name="cursor"></param>
    /// <param name="cancellationToken">token from caller</param>
    /// <param name="query"></param>
    /// <returns></returns>
    Task<List<Product>> SelectAsync(
        IEnumerable<long>? ids = null,
        OrderBy? orderBy = null,
        string? query = null,
        GetProductsGeneralHandler.SearchCursor? cursor = null,
        CancellationToken cancellationToken = default);
    //#TODO amas where filteric unda, yvelafers igebs viewdan

    /// <summary>
    ///     returns one row by id matching from VProducts
    /// </summary>
    /// <param name="id">id looking for</param>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<Product?> SelectAsync(long id, CancellationToken cancellationToken);

    Task<Product?> SelectAsync(string objId, CancellationToken cancellationToken);

    /// <summary>
    ///     adds new row in Products table.
    ///     product object then be populated
    /// </summary>
    /// <param name="product">transport object</param>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<Product> InsertAsync(Product product, CancellationToken cancellationToken);
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    ///     returns true if exists one Product matching by id
    /// </summary>
    /// <param name="id">id looking for</param>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken);
}