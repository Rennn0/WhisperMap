using XcLib.Data.ApplicationObjects;

namespace XcLib.Data.Abstractions;

public interface IProductRepo : IBasicRepo<Product>
{
    Task<List<Product>> GetAsync(
        IEnumerable<long>? ids = null,
        OrderBy? orderBy = null,
        string? query = null,
        SearchCursor? cursor = null,
        CancellationToken cancellationToken = default);

    Task<Product> GetByIdAsync(string objId, CancellationToken cancellationToken);
    
    Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken);
}