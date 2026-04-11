using XcLib.Data.ApplicationObjects;

namespace XcLib.Data.Abstractions;

public interface IProductCartRepo
{
    ValueTask<ProductCart> UpsertAsync(ProductCart productCart, CancellationToken cancellationToken);
    ValueTask<ProductCart?> SelectAsync(string userId, CancellationToken cancellationToken);
    ValueTask<ProductCart?> RemoveAsync(string userId, string productId, CancellationToken cancellationToken);

    ValueTask<ProductCart?> RemoveAsync(string userId, IEnumerable<string> productIds,
        CancellationToken cancellationToken);
}