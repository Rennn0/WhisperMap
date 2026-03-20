using XatiCraft.Data.Objects;

namespace XatiCraft.Data.Repos;

internal interface IProductCartRepo
{
    ValueTask<ProductCart> UpsertAsync(ProductCart productCart, CancellationToken cancellationToken);
    ValueTask<ProductCart?> SelectAsync(string userId, CancellationToken cancellationToken);
}