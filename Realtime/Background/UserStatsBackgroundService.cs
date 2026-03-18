using Realtime.Sse.Core.Stream;
using Realtime.Sse.Features.StreamData;
using Realtime.Sse.Features.StreamRegistries;
using Realtime.Sse.Formatters;

namespace Realtime.Background;

internal class UserStatsBackgroundService : BackgroundService
{
    private readonly SseUserStatsStreamRegistry _registry;
    private readonly IStreamDataProvider<SseUserStatsFormatter.UserStats> _streamDataProvider;

    public UserStatsBackgroundService(SseUserStatsStreamRegistry registry,
        IStreamDataProvider<SseUserStatsFormatter.UserStats> streamDataProvider)
    {
        _registry = registry;
        _streamDataProvider = streamDataProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SseStreamRegistry<SseUserStatsFormatter.UserStats>.StreamHandle streamHandle =
                _registry.GetStream("users", stoppingToken);

            if (streamHandle.SubscribersCount > 0)
            {
                SseUserStatsFormatter.UserStats stats = await _streamDataProvider.Instant(streamHandle, stoppingToken);
                await streamHandle.PublishAsync(stats, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}