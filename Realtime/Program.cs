using Realtime.Sse.Core.Stream;
using Realtime.Sse.Core.Streamer;
using Realtime.Sse.Features;
using Realtime.Sse.Formatters;

namespace Realtime;

public static class Program
{
    private static Timer? _timer;

    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddSingleton<SseStream<string>, ProductsSseStream>();
        builder.Services.AddSingleton<SseStream<float>, ProductsPriceSseStream>();

        WebApplication app = builder.Build();
        RouteGroupBuilder rtGroup = app.MapGroup("/realtime");
        rtGroup.MapGet("/", () => "xx");
        rtGroup.MapGet("/products",
            async (HttpContext ctx, SseStream<float> stream,
                CancellationToken cancellationToken) =>
            {
                // await using SseStream<float>.StreamSubscription subscription = stream.Subscribe();
                // SseSignalStreamer streamer = new SseSignalStreamer(ctx, cancellationToken);
                SseEnumerableStreamer streamer = new SseEnumerableStreamer(ctx, cancellationToken);
                SseStream<float>.StreamSubscription subscription = stream.Subscribe(cancellationToken);
                
                try
                {
                    // SseSignalRegistry<string>.SignalHandle signalHandle =
                    //     SseSignalRegistry<string>.GetSignal("luka", cancellationToken);
                    // signalHandle.Token.Register(() => Console.WriteLine("Token CAAANCELEd"));

                    await streamer.StreamAsync(
                        subscription.ReadAllAsync(cancellationToken),
                        "products-single",
                        TimeSpan.FromSeconds(5),
                        new SseDefaultFloatFormatter());

                    // await streamer.StreamAsync(
                    //     subscription.Reader,
                    //     "products",
                    //     TimeSpan.FromSeconds(3),
                    //     new SseDefaultFloatFormatter());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        AsyncLocal<SomeWrapper> counter = new AsyncLocal<SomeWrapper>
            { Value = new SomeWrapper { Val = 1 } };

        DateTimeOffset nwo = DateTimeOffset.Now;

        _timer = new Timer(async void (_) =>
        {
            try
            {
                // await using SseSignalRegistry<string>.SignalHandle signalHandle =
                //     SseSignalRegistry<string>.GetSignal("luka");
                // await signalHandle.PublishAsync(DateTimeOffset.Now.ToString());

                SseStream<float> fStream = app.Services.GetRequiredService<SseStream<float>>();
                await fStream.PublishAsync(counter.Value.Val++);

                if (nwo.AddSeconds(10) < DateTimeOffset.Now) await fStream.DisposeAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));

        await app.RunAsync();
    }

    private class SomeWrapper
    {
        public float Val { get; set; }
    }
}