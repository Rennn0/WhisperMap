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
            async (HttpContext ctx, CancellationToken cancellationToken) =>
            {
                SseEnumerableStreamer streamer = new SseEnumerableStreamer(ctx, cancellationToken);
                SseStream<string>.StreamSubscription subscription = stringStreamRegistry
                    .GetStream("luka", cancellationToken).Subscribe(cancellationToken);
                try
                {
                    await streamer.StreamAsync(
                        subscription.ReadAllAsync(cancellationToken),
                        "products-single",
                        TimeSpan.FromSeconds(2),
                        new SseDefaultStringFormatter());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        rtGroup.MapGet("/signal",
            async (HttpContext ctx, CancellationToken cancellationToken) =>
            {
                SseSignalStreamer streamer = new SseSignalStreamer(ctx, cancellationToken);
                SseSignalRegistry<string>.SignalHandle signalHandle =
                    stringSignalRegistry.GetSignal("luka", cancellationToken);

                try
                {
                    await streamer.StreamAsync(
                        signalHandle,
                        "products-single",
                        TimeSpan.FromSeconds(2),
                        new SseDefaultStringFormatter());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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