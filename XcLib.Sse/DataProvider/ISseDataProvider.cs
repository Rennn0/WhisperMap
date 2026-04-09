using XcLib.Sse.Core.Stream;

namespace XcLib.Sse.DataProvider;

public interface ISseDataProvider<T>
{
    ValueTask<T> GetAsync(CancellationToken cancellationToken = default);

    ValueTask<T> GetAsync(SseStreamRegistry<T>.StreamHandle streamHandle,
        CancellationToken cancellationToken = default);
}