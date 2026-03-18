using Realtime.Background;
using Realtime.Sse.Core.Stream;
using Realtime.Sse.Core.Streamer;
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

        RouteGroupBuilder realtimeGroup = app.MapGroup("/realtime");
        RouteGroupBuilder streamGroup = realtimeGroup.MapGroup("/stream");
        streamGroup.MapGet("/u", async (HttpContext context, SseUserStatsStreamRegistry registry) =>
            {
                CancellationToken cancellationToken = context.RequestAborted;
                SseEnumerableStreamer streamer = new SseEnumerableStreamer(context);
                SseStream<SseUserStatsFormatter.UserStats>.StreamSubscription subscription =
                    registry.GetStream("users", cancellationToken).Subscribe(cancellationToken);
                await streamer.StreamAsync(
                    subscription.ReadAllAsync(cancellationToken),
                    TimeSpan.FromSeconds(10),
                    new SseUserStatsFormatter());
            });

        // rtGroup.MapGet("/signal",
        //     async ctx =>
        //     {
        //         SseSignalStreamer streamer = new SseSignalStreamer(ctx);
        //         SseSignalRegistry<string>.SignalHandle signalHandle =
        //             stringSignalRegistry.GetSignal("luka", ctx.RequestAborted);
        //
        //         await streamer.StreamAsync(
        //             signalHandle,
        //             "signal",
        //             TimeSpan.FromSeconds(2),
        //             new SseDefaultStringFormatter());
        //     });

        await app.RunAsync();
    }
}