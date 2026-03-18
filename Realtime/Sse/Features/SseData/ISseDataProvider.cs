using Realtime.Sse.Core.Stream;

namespace Realtime.Sse.Features.SseData;

internal interface ISseDataProvider<T>
{
    ValueTask<T> Instant(CancellationToken cancellationToken = default);

    ValueTask<T> Instant(
        SseStreamRegistry<T>.StreamHandle streamHandle, CancellationToken cancellationToken = default);
}