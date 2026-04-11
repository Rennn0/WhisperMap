using Microsoft.AspNetCore.Mvc;
using Realtime.SseFeatures.Formatters;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Core.Streamer;
using XcLib.Sse.DataProvider;

namespace Realtime;

public static partial class Program
{
    private static void ApiGetSignal(this RouteGroupBuilder streamGroup) =>
        streamGroup.MapGet("/signal",
            async (HttpContext context,
                [FromQuery(Name = "sid")] string? streamId,
                [FromServices] SseSignalRegistry<UserStats> sseSignalRegistry,
                [FromKeyedServices(StreamerType.Signal)]
                SseStreamer<UserStats> streamer,
                [FromServices] ISseDataProvider<UserStats> sseDataProvider) =>
            {
                CancellationToken cancellationToken = context.RequestAborted;

                SseSignalRegistry<UserStats>.SignalHandle handle =
                    sseSignalRegistry.GetSignal("users", cancellationToken);

                await streamer.StreamAsync(handle);
            });
}