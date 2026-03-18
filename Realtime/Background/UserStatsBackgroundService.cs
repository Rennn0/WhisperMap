using Realtime.Sse.Core.Stream;
using Realtime.Sse.Features.StreamRegistries;
using Realtime.Sse.Formatters;

namespace Realtime.Background;

internal class UserStatsBackgroundService : BackgroundService
{
    private readonly SseUserStatsStreamRegistry _registry;

    public UserStatsBackgroundService(SseUserStatsStreamRegistry registry) => _registry = registry;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SseStreamRegistry<SseUserStatsFormatter.UserStats>.StreamHandle stream =
                _registry.GetStream("users", stoppingToken);

            if (stream.SubscribersCount > 0)
                await stream.PublishAsync(new SseUserStatsFormatter.UserStats
                {
                    Offline = Random.Shared.Next(0, 100), //#TODO real data
                    Online = stream.SubscribersCount
                }, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}