using Realtime.SseFeatures.Formatters;
using XcLib.Sse.Core.Stream;
using XcLib.Sse.DataProvider;

namespace Realtime.Background;

internal class UserStatsBackgroundService : BackgroundService
{
    private readonly ISseDataProvider<UserStats> _sseDataProvider;
    private readonly SseStreamRegistry<UserStats> _registry;
    private uint _c;

    public UserStatsBackgroundService(SseStreamRegistry<UserStats> sseStreamRegistry,
        ISseDataProvider<UserStats> sseDataProvider)
    {
        _registry = sseStreamRegistry;
        _sseDataProvider = sseDataProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SseStreamRegistry<UserStats>.StreamHandle streamHandle =
                _registry.GetStream("users", stoppingToken);

            if (streamHandle.SubscribersCount > 0)
            {
                Interlocked.Increment(ref _c);
                UserStats stats = await _sseDataProvider.GetAsync(streamHandle, stoppingToken);
                await streamHandle.PublishAsync(stats, stoppingToken);
            }

            // if (Volatile.Read(ref _c) > 3) await streamHandle.DisposeAsync();

            await Task.Delay(TimeSpan.FromSeconds(13), stoppingToken);
        }
    }
}