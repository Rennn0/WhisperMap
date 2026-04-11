using Microsoft.AspNetCore.Mvc;
using Realtime.SseFeatures.Formatters;
using XcLib.Sse.Core.Stream;
using XcLib.Sse.Core.Streamer;
using XcLib.Sse.DataProvider;

namespace Realtime;

public static partial class Program
{
    private static void ApiGetStream(this RouteGroupBuilder streamGroup) =>
        streamGroup.MapGet("/u",
            async (HttpContext context,
                [FromQuery(Name = "sid")] string? streamId,
                [FromServices] SseStreamRegistry<UserStats> sseStreamRegistry,
                [FromKeyedServices(StreamerType.Enumerable)]
                SseStreamer<UserStats> streamer,
                [FromServices] ISseDataProvider<UserStats> sseDataProvider) =>
            {
                CancellationToken cancellationToken = context.RequestAborted;
                SseStreamRegistry<UserStats>.StreamHandle handle =
                    sseStreamRegistry.GetStream("users", cancellationToken);
                SseStream<UserStats>.StreamSubscription subscription =
                    handle.Subscribe(streamId, cancellationToken);

                UserStats initialVal = await sseDataProvider.GetAsync(handle, cancellationToken);

                await streamer.StreamAsync(subscription.ReadAllAsync(cancellationToken), initialValue: initialVal);
            });
}