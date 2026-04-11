using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Realtime.SseFeatures.Formatters;
using XcLib.Sse.Configuration;
using XcLib.Sse.Core.Signal;

namespace Realtime;

public static partial class Program
{
    private static void ApiGetStreamCache(this RouteGroupBuilder streamGroup) =>
        streamGroup.MapGet("/cache", async (
            [FromServices] IDistributedCache cache,
            [FromServices] SseSignalRegistry<UserStats> signalReg,
            [FromServices] ILogger<WebApplication> logger,
            [FromServices] IConfigurationTrigger trigger,
            [FromQuery(Name = "k")] string key,
            [FromQuery(Name = "v")] string value) =>
        {
            // byte[]? c1 = cache.Get(key);
            // byte[]? c2 = cache.Get(key + "2");

            trigger.Invoke();

            await cache.SetAsync(key, Encoding.UTF8.GetBytes(value));
            await cache.SetAsync(key + "2", Encoding.UTF8.GetBytes(value), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });

            logger.LogWarning("hello there" + DateTimeOffset.UtcNow.ToLocalTime());

            await signalReg.GetSignal("users").PublishAsync(new UserStats { Offline = 99, Online = 33 });
            return "x";
        });
}