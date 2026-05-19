using XcLib.Data.ApplicationObjects;

namespace XcLib.Data.Abstractions;

public interface IBasicRepo<TObject> where TObject : ApplicationObject
{
    Task<TObject> AddAsync(TObject obj, CancellationToken token = default);
    Task<TObject?> GetByIdAsync(long id, CancellationToken token = default);
    Task<List<TObject>> GetAsync(TObject obj, ushort searchFlag = 0, CancellationToken token = default);
    Task<TObject?> UpdateAsync(TObject obj, CancellationToken token = default);
    Task DeleteAsync(TObject obj, CancellationToken token = default);
}