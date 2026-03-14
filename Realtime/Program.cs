using Realtime.Sse.Core.Signal;
using Realtime.Sse.Core.Stream;
using Realtime.Sse.Core.Streamer;
using Realtime.Sse.Features.SignalRegistries;
using Realtime.Sse.Features.StreamRegistries;
using Realtime.Sse.Formatters;

namespace Realtime;

public static class Program
{
    private static Timer? _timer;

    public static async Task Main(string[] args)
    {
        SseStringStreamRegistry stringStreamRegistry = new SseStringStreamRegistry();
        SseFloatStreamRegistry floatStreamRegistry = new SseFloatStreamRegistry();
        SseStringSignalRegistry stringSignalRegistry = new SseStringSignalRegistry();

        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
        if (File.Exists(swarmAppSettingsPath))
            builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

        WebApplication app = builder.Build();
        RouteGroupBuilder rtGroup = app.MapGroup("/realtime");
        rtGroup.MapGet("/", () => "xx");
        rtGroup.MapGet("/stream",
            async ctx =>
            {
                SseEnumerableStreamer streamer = new SseEnumerableStreamer(ctx);
                SseStream<string>.StreamSubscription subscription = stringStreamRegistry
                    .GetStream("luka", ctx.RequestAborted).Subscribe(ctx.RequestAborted);
                await streamer.StreamAsync(
                    subscription.ReadAllAsync(),
                    "stream",
                    TimeSpan.FromSeconds(2),
                    new SseDefaultStringFormatter());
            });

        rtGroup.MapGet("/signal",
            async ctx =>
            {
                SseSignalStreamer streamer = new SseSignalStreamer(ctx);
                SseSignalRegistry<string>.SignalHandle signalHandle =
                    stringSignalRegistry.GetSignal("luka", ctx.RequestAborted);

                await streamer.StreamAsync(
                    signalHandle,
                    "signal",
                    TimeSpan.FromSeconds(2),
                    new SseDefaultStringFormatter());
            });


        DateTimeOffset now = DateTimeOffset.Now;

        _timer = new Timer(async void (_) =>
        {
            try
            {
                SseStreamRegistry<string>.StreamHandle stream =
                    stringStreamRegistry.GetStream("luka");
                await stream.PublishAsync(DateTimeOffset.Now.ToString("D"));

                // if (now.AddSeconds(20) >= DateTimeOffset.Now) return;
                //
                // await stream.DisposeAsync();

                SseSignalRegistry<string>.SignalHandle signalHandle =
                    stringSignalRegistry.GetSignal("luka");
                await signalHandle.PublishAsync(DateTimeOffset.Now.ToString("R"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));

        await app.RunAsync();
    }
}