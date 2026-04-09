using Realtime.SseFeatures.Formatters;
using XcLib.Sse.Core.Stream;
using XcLib.Sse.DataProvider;

namespace Realtime.SseFeatures.SseData;

public class UserStatsProvider : ISseDataProvider<UserStats>
{
    public ValueTask<UserStats> GetAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public ValueTask<UserStats> GetAsync(
        SseStreamRegistry<UserStats>.StreamHandle streamHandle,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(new UserStats
        {
            Online = streamHandle.SubscribersCount,
            Offline = -1
        });
}