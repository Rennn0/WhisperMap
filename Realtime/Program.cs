using Microsoft.AspNetCore.Mvc;
using Realtime.Background;
using Realtime.Sse.Core.Stream;
using Realtime.Sse.Core.Streamer;
using Realtime.Sse.Features.SseData;
using Realtime.Sse.Features.StreamRegistries;
using Realtime.Sse.Formatters;

namespace Realtime;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        
        const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
        if (File.Exists(swarmAppSettingsPath))
            builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

        builder.Services.AddSingleton<SseUserStatsStreamRegistry>();

        builder.Services.AddTransient<ISseDataProvider<SseUserStatsFormatter.UserStats>, UserStatsProvider>();

        builder.Services.AddHostedService<UserStatsBackgroundService>();

        builder.Services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(pol =>
            {
                pol.AllowAnyHeader();
                pol.AllowAnyMethod();
                pol.AllowAnyOrigin();
            });
        });
        
        WebApplication app = builder.Build();
        app.UseCors();
        
        new Timer(_ =>
        {
            var rand = Random.Shared.Next(0, 3);
            if (rand > 1)
            {
                app.Logger.LogWarning(new EventId(rand),DateTimeOffset.Now.ToString());
            }
            else
            {
                app.Logger.LogInformation(new EventId(rand),DateTimeOffset.Now.ToString());
            }
        }).Change(1000, 1000);

        RouteGroupBuilder realtimeGroup = app.MapGroup("/realtime");
        RouteGroupBuilder streamGroup = realtimeGroup.MapGroup("/stream");
        streamGroup.MapGet("/u",
            async (HttpContext context,
                [FromQuery(Name = "sid")] string? streamId,
                SseUserStatsStreamRegistry registry,
                ISseDataProvider<SseUserStatsFormatter.UserStats> sseDataProvider) =>
            {
                CancellationToken cancellationToken = context.RequestAborted;
                SseStreamRegistry<SseUserStatsFormatter.UserStats>.StreamHandle handle =
                    registry.GetStream("users", cancellationToken);
                SseStream<SseUserStatsFormatter.UserStats>.StreamSubscription subscription =
                    handle.Subscribe(streamId, cancellationToken);
                SseUserStatsFormatter.UserStats
                    initialVal = await sseDataProvider.Instant(handle, cancellationToken);
                
                SseEnumerableStreamer streamer = new SseEnumerableStreamer(context);
                await streamer.StreamAsync(
                    subscription.ReadAllAsync(cancellationToken),
                    TimeSpan.FromSeconds(10),
                    new SseUserStatsFormatter(),
                    initialVal);
            });

        await app.RunAsync();
    }
}