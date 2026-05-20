using XcLib.Data.ApplicationObjects;

namespace XcLib.Data.Abstractions;

public interface IBasicRepo<TObject> where TObject : ApplicationObject
{
    Task<TObject> GetByIdAsync(long id, CancellationToken token = default);
    Task<TObject> AddAsync(TObject obj, CancellationToken token = default);
    Task<List<TObject>> GetAsync(TObject obj, ushort searchFlag = 0, CancellationToken token = default);
    Task<TObject?> UpdateAsync(TObject obj, ushort searchFlag = 0, CancellationToken token = default);
    Task<int> DeleteAsync(TObject obj, ushort searchFlag = 0, CancellationToken token = default);
    Task<bool> ExistsAsync(TObject obj, ushort searchFlag = 0, CancellationToken token = default);
}