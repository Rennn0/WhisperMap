using XatiCraft.Objects;

namespace XatiCraft.Data.Repos;

/// <summary>
///     defines functionality needed to manipulate ProductMetadata entity
/// </summary>
public interface IProductMetadaRepo
{
    /// <summary>
    ///     adds new row in ProductsMetadata table.
    ///     productMetadata object then be populated
    /// </summary>
    /// <param name="productMetadata">transport object</param>
    /// <param name="cancellationToken">token from caller</param>
    /// <returns></returns>
    Task<ProductMetadata> InsertAsync(ProductMetadata productMetadata, CancellationToken cancellationToken);
}