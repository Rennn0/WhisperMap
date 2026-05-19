using XcLib.Data.ApplicationObjects;

namespace XcLib.Data.Abstractions;

public interface IBasicRepo<TEntity, in TObject> where TObject : ApplicationObject
{
    Task<TEntity> AddAsync(TObject obj, CancellationToken token = default);
    Task<TEntity?> GetAsync(TObject obj, ushort searchFlag = 0, CancellationToken token = default);
    Task<TEntity?> UpdateAsync(TObject obj, CancellationToken token = default);
    Task DeleteAsync(TObject obj, CancellationToken token = default);
}