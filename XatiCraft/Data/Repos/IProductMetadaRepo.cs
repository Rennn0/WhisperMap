using XatiCraft.Objects;

namespace XatiCraft.Data.Repos;

public interface IProductMetadaRepo
{
    Task<ProductMetadata> InsertAsync(ProductMetadata productMetadata, CancellationToken cancellationToken);
}