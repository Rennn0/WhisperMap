using Realtime.Sse.Core.Stream;

namespace Realtime.Sse.Features.StreamData;

internal interface IStreamDataProvider<T>
{
    ValueTask<T> Instant(CancellationToken cancellationToken = default);

    ValueTask<T> Instant(
        SseStreamRegistry<T>.StreamHandle streamHandle, CancellationToken cancellationToken = default);
}