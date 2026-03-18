using Realtime.Sse.Core.Stream;
using Realtime.Sse.Features.SseData;
using Realtime.Sse.Features.StreamRegistries;
using Realtime.Sse.Formatters;

namespace Realtime.Background;

internal class UserStatsBackgroundService : BackgroundService
{
    private readonly SseUserStatsStreamRegistry _registry;
    private readonly ISseDataProvider<SseUserStatsFormatter.UserStats> _sseDataProvider;

    public UserStatsBackgroundService(SseUserStatsStreamRegistry registry,
        ISseDataProvider<SseUserStatsFormatter.UserStats> sseDataProvider)
    {
        _registry = registry;
        _sseDataProvider = sseDataProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SseStreamRegistry<SseUserStatsFormatter.UserStats>.StreamHandle streamHandle =
                _registry.GetStream("users", stoppingToken);

            if (streamHandle.SubscribersCount > 0)
            {
                SseUserStatsFormatter.UserStats stats = await _sseDataProvider.Instant(streamHandle, stoppingToken);
                await streamHandle.PublishAsync(stats, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}