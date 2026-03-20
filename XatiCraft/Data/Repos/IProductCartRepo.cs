using XatiCraft.Data.Objects;

namespace XatiCraft.Data.Repos;

internal interface IProductCartRepo
{
    ValueTask<ProductCart> UpsertAsync(ProductCart productCart, CancellationToken cancellationToken);
    ValueTask<ProductCart?> SelectAsync(string userId, CancellationToken cancellationToken);
    ValueTask<ProductCart?> RemoveAsync(string userId, string productId, CancellationToken cancellationToken);

    ValueTask<ProductCart?> RemoveAsync(string userId, IEnumerable<string> productIds,
        CancellationToken cancellationToken);
}