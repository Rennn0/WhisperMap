using Realtime.Sse.Core.Stream;
using Realtime.Sse.Formatters;

namespace Realtime.Sse.Features.StreamData;

internal class UserStatsProvider : IStreamDataProvider<SseUserStatsFormatter.UserStats>
{
    public ValueTask<SseUserStatsFormatter.UserStats> Instant(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public ValueTask<SseUserStatsFormatter.UserStats> Instant(
        SseStreamRegistry<SseUserStatsFormatter.UserStats>.StreamHandle streamHandle,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(new SseUserStatsFormatter.UserStats
        {
            Online = streamHandle.SubscribersCount,
            Offline = -1
        });
}